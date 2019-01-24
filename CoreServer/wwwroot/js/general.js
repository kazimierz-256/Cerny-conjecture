
let stats = new Stats();
let appSettings = new settings(1);

let camera, scene, renderer, controls, mesh, water, cubeCamera, existingGraph, skyParameters, sunLight;
let generateAction = undefined;
let cameraDistance = 3;
const zoomFactor = 2;
let animatables = [];

let px = 0, py = 0, pz = 0;
let vx = 0, vy = 0, vz = 0;
let pvx = 0, pvy = 0, pvz = 0;
let pax = 0, pay = 0, paz = 0;
let maxDisplacement = 12;

let previousTimestamp = 0;
let firstRun = true;

let numberCount = 1;
let arrayX = new Float32Array(new ArrayBuffer(4 * numberCount));
let arrayY = new Float32Array(new ArrayBuffer(4 * numberCount));
let arrayZ = new Float32Array(new ArrayBuffer(4 * numberCount));
let consideringIndex = 0;
let averageX = 0.0;
let averageY = 0.0;
let averageZ = 0.0;
const directionThreshold = 0.7;
const lengthThreshold = 5;
const timeout = 1000;
let latestActionTimestamp = 0;

let fontLoader = new THREE.FontLoader();

let startProcessingSensorData = () => {
    let acl = new LinearAccelerationSensor({ frequency: 60 });
    acl.addEventListener('activate', () => {
        onreading = () => {
            let elapsedTime = (acl.timestamp - previousTimestamp) / 1000;

            averageX -= arrayX[consideringIndex];
            averageY -= arrayY[consideringIndex];
            averageZ -= arrayZ[consideringIndex];

            arrayX[consideringIndex] = acl.x;
            arrayY[consideringIndex] = acl.y;
            arrayZ[consideringIndex] = acl.z;

            averageX += arrayX[consideringIndex];
            averageY += arrayY[consideringIndex];
            averageZ += arrayZ[consideringIndex];

            consideringIndex++;
            if (consideringIndex == numberCount)
                consideringIndex = 0;

            let accelerationVector = new THREE.Vector3(
                averageX / numberCount,
                averageY / numberCount,
                averageZ / numberCount
            );
            let unitAcceleration = new THREE.Vector3(
                averageX,
                averageY,
                averageZ
            ).normalize();
            let moveNow = false;
            let moveVector = new THREE.Vector3(accelerationVector.x, accelerationVector.y, 0);
            accelerationVector.applyQuaternion(camera.quaternion);

            let ax = accelerationVector.x;
            let ay = accelerationVector.y;
            let az = accelerationVector.z;

            appSettings.accelX = ax;
            appSettings.accelY = ay;
            appSettings.accelZ = az;
            
            if (!firstRun) {

                let newCameraPosition =
                    new THREE.Vector3(0, 0, cameraDistance)
                        .applyQuaternion(camera.quaternion);

                let waterlimit = water.position.y * 0.9;
                if (newCameraPosition.y < waterlimit) {
                    newCameraPosition =
                        new THREE.Vector3(0, 0, cameraDistance * Math.abs(waterlimit / newCameraPosition.y))
                            .applyQuaternion(camera.quaternion);
                }
                camera.position.set(
                    newCameraPosition.x,
                    newCameraPosition.y,
                    newCameraPosition.z
                );
            } else {
                firstRun = false;
            }
            previousTimestamp = acl.timestamp;
        }

        acl.addEventListener('reading', onreading);
    });
    acl.addEventListener('error', error => {
        console.error("Cannot fetch data from sensor due to an error.");
    });
    acl.start();
}

let updateAnimatingControls = () => {
    if (appSettings.showPowerAutomaton) {
        if (appSettings.maximumConsideringDepth >= appSettings.maxDistance) {
            appSettings.maximumConsideringDepth = appSettings.maxDistance;
            $("#animateForward").addClass("disabled");
            $("#animateBack").removeClass("disabled");
        } else if (appSettings.maximumConsideringDepth <= 1) {
            appSettings.maximumConsideringDepth = 1;
            $("#animateForward").removeClass("disabled");
            $("#animateBack").addClass("disabled");
        } else {
            $("#animateForward").removeClass("disabled");
            $("#animateBack").removeClass("disabled");
        }
        $("#animateForward").show();
        $("#animateBack").show();
    } else {
        $("#animateForward").hide();
        $("#animateBack").hide();
    }
};

let updateAnimatingButtons = () => {
    $("#innerText").text(appSettings.animating ? "portable_wifi_off" : "wifi_tethering");
}

let lastRecordedMovement = {};
let animations = {};
let animatingTowardsON = {};
let iteration = 0;

let focusAnimation = { foo: 0.0 };
let mouseDownConsideringVertex = -1;
let allowVertexFocus = 0;

let travelToVertex = (toVertex, easing, duration, onComplete) => {
    if (travelTimeout != undefined)
        clearTimeout(travelTimeout);
    appSettings.previousVertexFocus = appSettings.currentVertexFocus;
    appSettings.currentVertexFocus = toVertex;
    $(focusAnimation).stop(true, false);
    focusAnimation.foo = appSettings.transitionFocusFraction = 0.0;
    $(focusAnimation).animate({ foo: 1.0 }, {
        duration: duration == undefined ? appSettings.moveFocusTimout : duration,
        easing: easing == undefined ? appSettings.smoothOutEasing : easing,
        step: (now) => {
            appSettings.transitionFocusFraction = now;
        },
        complete: () => {
            if (onComplete != undefined)
                onComplete();
        }
    });
}

let flatForce = { foo: 0.0 };
let stayFlat = false;
let toggleFlatForce = (isOn) => {
    $(flatForce).stop(true, false);
    $(flatForce).animate({ foo: isOn ? 1.0 : 0.0 }, {
        duration: appSettings.flatteningForceTimeout,
        easing: appSettings.flatteningEasing,
        step: (now) => {
            appSettings.flatteningForceFraction = now;
        }
    });
}
let threeDimForce = { foo: 0.0 };
let togglethreeDimForce = (isOn) => {
    $(threeDimForce).stop(true, false);
    $(threeDimForce).animate({ foo: isOn ? 1.0 : 0.0 }, {
        duration: appSettings.flatteningForceTimeout,
        easing: appSettings.flatteningEasing,
        step: (now) => {
            appSettings.threeDimForceFraction = now;
        }
    });
}

let travelTimeout = undefined;
let init = (createControlFromCamera) => {

    renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setPixelRatio(window.devicePixelRatio);
    renderer.setSize(window.innerWidth, window.innerHeight);
    document.body.appendChild(renderer.domElement);

    camera = new THREE.PerspectiveCamera(appSettings.exploratoryFOV, window.innerWidth / window.innerHeight, 0.2, 10000);

    appSettings.camera = camera;
    createControlFromCamera(camera);

    scene = new THREE.Scene();
    

    sunLight = new THREE.DirectionalLight(0xffffff, 1);
    scene.add(sunLight);

    let waterGeometry = new THREE.PlaneBufferGeometry(10000, 10000);
    water = new THREE.Water(
        waterGeometry,
        {
            textureWidth: 512,
            textureHeight: 512,
            waterNormals: new THREE.TextureLoader().load('textures/waternormals.jpg', function (texture) {
                texture.wrapS = texture.wrapT = THREE.RepeatWrapping;
            }),
            alpha: 0.8,
            sunDirection: sunLight.position.clone().normalize(),
            sunColor: sunLight.color,
            waterColor: 0x001e0f,
            distortionScale: 1,
            fog: scene.fog !== undefined
        }
    );
    water.material.uniforms.size.value *= 4;
    water.rotation.x = - Math.PI / 2;
    water.position.y = -5;
    scene.add(water);

    var sky = new THREE.Sky();
    sky.scale.setScalar(100000);
    scene.add(sky);
    var uniforms = sky.material.uniforms;
    // midnight
    // uniforms.turbidity.value = 200;
    // uniforms.rayleigh.value = 0.05;
    // uniforms.luminance.value = 1;
    // uniforms.mieCoefficient.value = 0.00001;
    // uniforms.mieDirectionalG.value = 0.25;

    // day
    uniforms.turbidity.value = 10;
    uniforms.rayleigh.value = 1;
    uniforms.luminance.value = 1;
    uniforms.mieCoefficient.value = 0.005;
    uniforms.mieDirectionalG.value = 0.85;
    skyParameters = {
        distance: 400,
        inclination: -0.3,
        azimuth: 0.1
    };

    updateSun = () => {
        var theta = Math.PI * (skyParameters.inclination - 0.5);
        var phi = 2 * Math.PI * (skyParameters.azimuth - 0.5);
        sunLight.position.x = skyParameters.distance * Math.cos(phi);
        sunLight.position.y = skyParameters.distance * Math.sin(phi) * Math.sin(theta);
        sunLight.position.z = skyParameters.distance * Math.sin(phi) * Math.cos(theta);
        sky.material.uniforms.sunPosition.value = sunLight.position.copy(sunLight.position);
        water.material.uniforms.sunDirection.value.copy(sunLight.position).normalize();
    }

    
    animatables.forEach(animatable => animatable.init(scene, appSettings));

    // initialize material design
    M.Dropdown.init(document.querySelectorAll('.dropdown-trigger'), {});
    M.Modal.init(document.querySelectorAll('.modal'), {
        onOpenStart: () => {
            $("#controls").css("z-index", 0);
            allowVertexFocus += 1;
            toggleCanvasBlur(true, 500, appSettings.smallBlur);
        },
        onCloseStart: () => {
            controls.enabled = true;
            toggleCanvasBlur(false, 500);
        },
        onCloseEnd: () => {
            $("#controls").css("z-index", 20000);
            allowVertexFocus -= 1;
            controls.enabled = true;
        }
    });
    M.TapTarget.init(document.querySelectorAll('.tap-target'), {});
    M.FormSelect.init(document.querySelectorAll('select'), {});
    M.FloatingActionButton.init(document.querySelectorAll('.fixed-action-btn'), { hoverEnabled: false, toolbarEnabled: true });
    M.Collapsible.init(document.querySelectorAll('.collapsible'), {});
    M.Range.init(document.querySelectorAll('input[type=range]'));

    // add custom functionality
    $("#animate-shortest-path").click(() => {
        if (appSettings.shortestPath == undefined)
            return;
        let animateFurther = Array(appSettings.shortestPath.length);
        for (let i = 0; i < appSettings.shortestPath.length; i++) {
            animateFurther[i] = () => {
                if (travelTimeout != undefined)
                    clearTimeout(travelTimeout);
                travelToVertex(appSettings.shortestPath[i], appSettings.travelEasing, undefined, animateFurther[i - 1]);
            }
        }
        animateFurther[animateFurther.length - 1]();
    });


    let dollyAnimation = { foo: cameraDistance };
    let dolly = (targetScale) => {
        $(dollyAnimation).stop(true, false);
        dollyAnimation.foo = cameraDistance;
        $(dollyAnimation).animate({ foo: targetScale }, {
            duration: appSettings.zoomTimeout * 10,
            easing: appSettings.zoomEasing,
            step: (now) => {
                cameraDistance = now;
            }
        });
    }
    $("#showPowerGraph").click((e) => {
        appSettings.showPowerAutomaton = true;
        $("#showPowerGraph").hide();
        $("#showNormalNotPowerGraph").show();

        if (generateAction != undefined)
            showGraph(generateAction());
    });
    $("#showNormalNotPowerGraph").click((e) => {
        appSettings.showPowerAutomaton = false;
        $("#showPowerGraph").show();
        $("#showNormalNotPowerGraph").hide();

        if (generateAction != undefined)
            showGraph(generateAction());
    });
    $("#flat3d").click((e) => {
        stayFlat = !stayFlat;
        toggleFlatForce(stayFlat);

        $("#flat3dText").text(stayFlat ? "3d_rotation" : "vertical_align_center");
        M.toast({ html: stayFlat ? "Flattening force activated" : "Flattening force deactivated (please wait for 3d recovery)", displayLength: 2000 });
        e.stopPropagation();
    });
    let distanceAnimation = { foo: 0.0 };
    $("#animateBack").click((e) => {
        appSettings.previousMaximumConsideringDepth = appSettings.maximumConsideringDepth;
        appSettings.maximumConsideringDepth -= 1;
        updateAnimatingControls();
        M.toast({ html: "BFS step " + (appSettings.maximumConsideringDepth - 1), displayLength: 2000 });

        $(distanceAnimation).stop(true, false);
        distanceAnimation.foo = appSettings.distanceFocusFraction = 0.0;
        $(distanceAnimation).animate({ foo: 1.0 }, {
            duration: appSettings.moveFocusTimout,
            easing: appSettings.smoothOutEasing,
            step: (now) => {
                appSettings.distanceFocusFraction = now;
            }
        });

        e.stopPropagation();
    });
    $("#animateForward").click((e) => {
        appSettings.previousMaximumConsideringDepth = appSettings.maximumConsideringDepth;
        appSettings.maximumConsideringDepth += 1;
        updateAnimatingControls();
        M.toast({ html: "BFS step " + (appSettings.maximumConsideringDepth - 1), displayLength: 2000 });

        $(distanceAnimation).stop(true, false);
        distanceAnimation.foo = appSettings.distanceFocusFraction = 0.0;
        $(distanceAnimation).animate({ foo: 1.0 }, {
            duration: appSettings.moveFocusTimout,
            easing: appSettings.smoothOutEasing,
            step: (now) => {
                appSettings.distanceFocusFraction = now;
            }
        });

        e.stopPropagation();
    });
    $("#fullscreen").click(() => {
        let doc = window.document;
        let docEl = doc.documentElement;

        let requestFullScreen = docEl.requestFullscreen || docEl.mozRequestFullScreen || docEl.webkitRequestFullScreen || docEl.msRequestFullscreen;
        let cancelFullScreen = doc.exitFullscreen || doc.mozCancelFullScreen || doc.webkitExitFullscreen || doc.msExitFullscreen;

        if (!doc.fullscreenElement && !doc.mozFullScreenElement && !doc.webkitFullscreenElement && !doc.msFullscreenElement) {
            requestFullScreen.call(docEl);
            $("#fullscreenText").text("fullscreen_exit");
        }
        else {
            cancelFullScreen.call(doc);
            $("#fullscreenText").text("fullscreen");
        }
    });

    window.addEventListener('resize', onWindowResize, false);

    $('.modal-close').click(() => {
        controls.enabled = true;
    });
    $('.modal-trigger').click(() => {
        controls.enabled = false;
    });

    let highlightVertex = (key, doHighlight) => {
        let foundObjectMaterial = lastRecordedMovement[key][1];
        let savedColor = lastRecordedMovement[key][2];
        $(animations[key]).stop(false, false);
        $(animations[key]).animate({ foo: doHighlight ? appSettings.highlighted : appSettings.withoutHighlight }, {
            duration: doHighlight ?
                (appSettings.highlightTimeout * (animations[key].foo - appSettings.highlighted) / (appSettings.withoutHighlight - appSettings.highlighted)) :
                (appSettings.dehighlightTimeout * (appSettings.withoutHighlight - animations[key].foo) / (appSettings.withoutHighlight - appSettings.highlighted)),
            easing: appSettings.smoothOutEasing,
            step: (now) => {
                foundObjectMaterial.emissive.setHSL(savedColor.h, savedColor.s,
                    savedColor.l * now + 1 * (1 - now));
            }
        });
        animatingTowardsON[key] = doHighlight;
    }

    setInterval(() => {
        if (!mouseMoved || 0 != allowVertexFocus)
            return;

        iteration += 1;
        raycaster.setFromCamera(mouse, camera);

        // calculate objects intersecting the picking ray
        let intersects = raycaster.intersectObjects(scene.children, true);

        let found = false;
        for (let i = 0; i < intersects.length; i++) {
            if (intersects[i].object.automatonId != undefined && intersects[i].object.material.defaultColor != undefined) {
                found = true;
                if (lastRecordedMovement[intersects[i].object.automatonId] == undefined) {
                    let savedColor = new THREE.Color();
                    let foundObjectMaterial = intersects[i].object.material;
                    foundObjectMaterial.emissive.getHSL(savedColor);
                    lastRecordedMovement[intersects[i].object.automatonId] = [iteration, foundObjectMaterial, savedColor];
                    animations[intersects[i].object.automatonId] = { foo: appSettings.withoutHighlight };
                } else {
                    lastRecordedMovement[intersects[i].object.automatonId][0] = iteration;
                }
            }
        }

        for (let key in lastRecordedMovement) {

            let objectIteration = lastRecordedMovement[key][0];

            if (objectIteration == iteration) {
                // is currently selected
                if (animatingTowardsON[key] == undefined) {
                    animatingTowardsON[key] = false;
                }

                if (animatingTowardsON[key] === true) {
                    // if animating to ON then ignore
                } else {
                    // if animating to OFF then stop and begin animating towards ON
                    highlightVertex(key, true);
                }
            } else if (objectIteration < iteration) {
                // remove highlight
                if (animatingTowardsON[key] == undefined) {
                    animatingTowardsON[key] = true;
                }

                if (animatingTowardsON[key] === true) {
                    // if animating to ON then stop and begin animating towards OFF
                    highlightVertex(key, false);
                    animatingTowardsON[key] = false;
                } else {
                    // if animating to OFF then ignore
                }
            }
        }

        $(document.body).css("cursor", found ? "pointer" : "initial");

    }, 60);


    $("body > canvas, #vignette").mousedown((e) => {
        if (0 != allowVertexFocus)
            return;

        mouse.x = (e.clientX / window.innerWidth) * 2 - 1;
        mouse.y = - (e.clientY / window.innerHeight) * 2 + 1;
        raycaster.setFromCamera(mouse, camera);

        // calculate objects intersecting the picking ray
        let intersects = raycaster.intersectObjects(scene.children, true);
        mouseDownConsideringVertex = -1;
        let bestDistance = Infinity;
        for (let i = 0; i < intersects.length; i++) {
            // intersects[i].object.material.color.set(0xff0000);
            if (intersects[i].object.automatonId != undefined) {
                if (intersects[i].distance < bestDistance) {
                    mouseDownConsideringVertex = intersects[i].object.automatonId;
                    $("#resetCenterOfMass").removeClass("disabled");
                    bestDistance = intersects[i].distance;
                }
            }
        }
    });

    $(document.body).mouseup((e) => {
        if (0 != allowVertexFocus)
            return;

        mouse.x = (e.clientX / window.innerWidth) * 2 - 1;
        mouse.y = - (e.clientY / window.innerHeight) * 2 + 1;
        raycaster.setFromCamera(mouse, camera);

        // calculate objects intersecting the picking ray
        let intersects = raycaster.intersectObjects(scene.children, true);
        let mouseUpConsideringVertex = -1;
        let bestDistance = Infinity;
        for (let i = 0; i < intersects.length; i++) {
            // intersects[i].object.material.color.set(0xff0000);
            if (intersects[i].object.automatonId != undefined) {
                if (intersects[i].distance < bestDistance) {
                    mouseUpConsideringVertex = intersects[i].object.automatonId;
                    $("#resetCenterOfMass").removeClass("disabled");
                    bestDistance = intersects[i].distance;
                }
            }
        }

        if (bestDistance != Infinity && mouseUpConsideringVertex == mouseDownConsideringVertex) {
            travelToVertex(mouseDownConsideringVertex, appSettings.smoothOutEasing);
            M.toast({ html: "The camera is now focusing at vertex " + (mouseDownConsideringVertex >>> 0).toString(2).padStart(appSettings.n, "0"), displayLength: 1000 })
        }
    });
    

    stats.showPanel(0);
    $(stats.domElement).css("opacity", 0);

    document.body.appendChild(stats.dom);

    $("#pauseplay").on("click",
        (e) => {
            appSettings.animating = !appSettings.animating;
            M.toast({ html: appSettings.animating ? "Animation playing" : "Animation paused", displayLength: 1000 })

            updateAnimatingButtons()
            e.stopPropagation();
        }
    );
    updateAnimatingButtons();

    $("#random-graph-generate").on("click", () => {
        generateAction = () => graphs.getRandomAutomaton($("#random-graph-size").val(), appSettings, cubeCamera);
        showGraph(generateAction());
    });
    $("#cerny-graph-generate").on("click", () => {
        generateAction = () => graphs.getCernyAutomaton($("#cerny-graph-size").val(), appSettings, cubeCamera);
        showGraph(generateAction());
    });
    $("#custom-graph-generate").on("click", () => {
        generateAction = () => parseGraph($("#custom-graph-transitions").val(), cubeCamera);
        showGraph(generateAction());
    });
    $("#generate-karis-automaton").on("click", () => {
        generateAction = () => graphs.getKarisAutomaton(appSettings, cubeCamera);
        showGraph(generateAction());
    });
    $("#generate-extreme-3-automaton").on("click", () => {
        generateAction = () => graphs.getExtreme3Automaton(appSettings, cubeCamera);
        showGraph(generateAction());
    });
    $("#generate-extreme-4-automaton").on("click", () => {
        generateAction = () => graphs.getExtreme4Automaton(appSettings, cubeCamera);
        showGraph(generateAction());
    });

    $("#quality-smooth").on("click", () => {
        appSettings.quality = 5;
        if (generateAction != undefined)
            showGraph(generateAction());
        M.toast({ html: "Realistically smooth quality setting will be applied once a new automaton is created", displayLength: 4000 })
    });

    $("#quality-ultra").on("click", () => {
        appSettings.quality = 4;
        if (generateAction != undefined)
            showGraph(generateAction());
        M.toast({ html: "Ultra quality setting will be applied once a new automaton is created", displayLength: 4000 })
    });

    $("#quality-standard").on("click", () => {
        appSettings.quality = 3;
        if (generateAction != undefined)
            showGraph(generateAction());
        M.toast({ html: "Standard quality setting will be applied once a new automaton is created", displayLength: 4000 })
    });

    $("#quality-low").on("click", () => {
        appSettings.quality = 1;
        if (generateAction != undefined)
            showGraph(generateAction());
        M.toast({ html: "Low quality setting will be applied once a new automaton is created", displayLength: 4000 })
    });

    $("#quality-minimum").on("click", () => {
        appSettings.quality = 0;
        if (generateAction != undefined)
            showGraph(generateAction());
        M.toast({ html: "Minimum quality setting will be applied once a new automaton is created", displayLength: 4000 })
    });

    $("#automaton-lights").change((e) => {
        appSettings.shineLights = e.target.checked;
        if (generateAction != undefined)
            showGraph(generateAction());
        if (appSettings.shineLights)
            M.toast({ html: "Lights will be available once a new automaton is created", displayLength: 4000 })
    });
    
    $("#automaton-speedup").change((e) => {
        appSettings.speedup = parseFloat(e.target.options[e.target.selectedIndex].value);
    });
    $("#automaton-repel").on("input", (e) => {
        appSettings.repellingConstant = e.target.value * appSettings.forceStrength;
    });
    $("#daytime").on("input", (e) => {
        appSettings.daytime = e.target.value;
        setMood(appSettings.daytime);
    });
    $("#automaton-string").on("input", (e) => {
        appSettings.stringConstant = e.target.value * appSettings.forceStrength;
    });
    $("#automaton-unique-string").on("input", (e) => {
        appSettings.uniqueStringConstant = e.target.value * appSettings.stringConstant;
    });
    $("#automaton-friction").on("input", (e) => {
        appSettings.friction = e.target.value * appSettings.forceStrength;
    });
    $("#string-length").on("input", (e) => {
        appSettings.targetStringLength = e.target.value;
    });


    let raycaster = new THREE.Raycaster();
    let mouse = new THREE.Vector2();
    let mouseMoved = false;
    $(document.body).mousemove((e) => {
        mouse.x = (e.clientX / window.innerWidth) * 2 - 1;
        mouse.y = - (e.clientY / window.innerHeight) * 2 + 1;
        mouseMoved = true;
    });

    $("#resetCenterOfMass").click((e) => {
        travelToVertex(-1);
        $("#resetCenterOfMass").addClass("disabled");
        M.toast({ html: "The camera is now focusing at the center of mass", displayLength: 1000 });
        e.stopPropagation();
    });

    setTimeout(() => {
        $("body > canvas").fadeTo(3000, 1, "swing");
    }, 1000);
    setTimeout(() => {
        $(stats.domElement).hover(
            () => {
                $(stats.domElement).fadeTo(200, 1, "swing");
            },
            () => {
                $(stats.domElement).fadeTo(1000, 0, "swing");
            }
        );

        toggleCanvasBlur(false);
    }, 1000);
    toggleCanvasBlur(true, 0, appSettings.maxBlur);

    setTimeout(() => {
        $(".preloader-wrapper").fadeTo(500, 0, "swing", () => {
            $(".preloader-wrapper").hide();
        });
        setTimeout(() => {
            $("#controls").fadeTo(500, 1);
        }, 500);
    }, 0);
}

let blurClearTimeout;
let previouslyBlurred = true;
let previousBlur = appSettings.maxBlur;
let blurAnimation = { foo: previousBlur };
let toggleCanvasBlur = (makeBlurred, timeout, blurAmount) => {
    
};

let parseGraph = (specification) => {
    let parsedGraph = JSON.parse(specification);
    return getAnimatableGraph(parsedGraph, appSettings, (appSettings.showPowerAutomaton ? "Power graph of an automaton" : "Automaton") + " of size " + Math.min(parsedGraph[0].length, parsedGraph[1].length), cubeCamera);
};


let animate = () => {
    let beforeRender = window.performance.now();
    stats.end();
    stats.begin();
    frameCount += 1;
    let time = frameCount / appSettings.targetFPS;
    water.material.uniforms.time.value = time / 3;
    animatables.forEach(animatable => animatable.update(time, appSettings, renderer, scene));
    controls.update();
    // make sure the camera doesn't go underwater
    let waterlimit = water.position.y * 0.9;
    if (camera.position.y < waterlimit) {
        camera.position.y = waterlimit;
    }
    if (takeScreenshot) {
        takeScreenshot = false;
        let w = $("body > canvas")[0].width;
        let h = $("body > canvas")[0].height;
        let desiredWidth = parseInt(window.prompt(`Please enter the desired width in pixels`, "2000"));
        desiredWidth = Math.floor(desiredWidth / window.devicePixelRatio);
        if (!isNaN(desiredWidth)) {
            let desiredHeight = Math.floor(desiredWidth * h / w);
            renderer.setSize(desiredWidth, desiredHeight);
            renderer.render(scene, camera);
            downloadURI($("body > canvas")[0].toDataURL(), "poster.png");
            onWindowResize();
        }
    } else {
        renderer.render(scene, camera);
    }
    let defaultTimeout = 1000.0 / appSettings.targetFPS;
    let timeout = Math.max(0, defaultTimeout - (window.performance.now() - beforeRender));
    camera.updateProjectionMatrix();
    setTimeout(() => window.requestAnimationFrame(animate), timeout);
}

let onWindowResize = () => {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
}

let resizeToGoldenRatio = () => {
    let w = window.innerWidth;
    let h = window.innerHeight;
    if (w > h * Math.sqrt(2))
        w = Math.ceil(h * Math.sqrt(2));
    else
        h = Math.ceil(w / Math.sqrt(2));
    camera.aspect = w / h;
    camera.updateProjectionMatrix();
    renderer.setSize(w, h);

    M.toast({ html: "The canvas proportions are now 1 to square root of 2. A3, A2 poster format. Press 's' to make a screenshot.", displayLength: 3000 });
}

let resizeToInverseGoldenRatio = () => {
    let w = window.innerWidth;
    let h = window.innerHeight;
    if (Math.floor(w * Math.sqrt(2)) > h)
        w = Math.floor(h / Math.sqrt(2));
    else
        h = Math.floor(w * Math.sqrt(2));
    camera.aspect = w / h;
    camera.updateProjectionMatrix();
    renderer.setSize(w, h);

    M.toast({ html: "The canvas proportions are now square root of 2 to 1. A3, A2 poster format. Press 's' to make a screenshot.", displayLength: 3000 });
}

$(document).ready(() => {
    fontLoader.load('fonts/LM Roman 10_Regular.json', (font) => {
        appSettings.font = font;

        let request = {};
        let pairs = location.search.substring(1).split('&');
        for (let i = 0; i < pairs.length; i++) {
            let pair = pairs[i].split('=');
            request[pair[0]] = pair[1];
        }
        let showGraphFromRequest = () => {
            if (request["quality"] != undefined)
                appSettings.quality = parseInt(request["quality"]);
            if (request["fps"] != undefined)
                appSettings.targetFPS = parseInt(request["fps"]);
            if (request["probability"] != undefined)
                appSettings.probabilityOfUpdate = parseFloat(request["probability"]);

            if (request["powerGraph"] != undefined)
                appSettings.showPowerAutomaton = 0 !== parseInt(request["powerGraph"]);

            $(appSettings.showPowerAutomaton ? "#showPowerGraph" : "#showNormalNotPowerGraph").click();

            if (request["automaton"] == undefined) {
                generateAction = () => graphs.getCernyAutomaton(4, appSettings, cubeCamera);
            } else {
                generateAction = () => parseGraph(unescape(request["automaton"]));
            }

            showGraph(generateAction());
        };

        if ('LinearAccelerationSensor' in window && ('ontouchstart' in window)) {
            navigator.permissions.query({ name: "accelerometer" }).then(result => {
                if (result.state != 'granted') {
                    document.title = "Sorry, we're not allowed to access sensors " +
                        "on your device..";
                    return;
                }
                init(camera => controls = new THREE.DeviceOrientationControls(camera));
                // for mobile devices
                if (!('ontouchstart' in window)) {
                    camera.position.setY(2);
                    camera.position.setX(4);
                }
                showGraphFromRequest();
                animate();
                startProcessingSensorData();
            });
        } else {
            init(camera => {
                controls = new THREE.OrbitControls(camera);
                controls.enableDamping = true;
                controls.dampingFactor = 0.3;
                controls.maxPolarAngle = Math.PI * 0.7;
                controls.minPolarAngle = Math.PI * 0.15;
                controls.minDistance = 0.7;
                controls.maxDistance = 100;
                controls.zoomSpeed = 2;
            });

            if (!('ontouchstart' in window)) {
                camera.position.setY(2);
                camera.position.setX(4);
            } else {
                camera.position.setY(2);
                camera.position.setX(4);
            }
            
            showGraphFromRequest();
            animate();
        }

        // for mobile devices
        if (!('ontouchstart' in window)) {
            camera.position.setY(2);
            camera.position.setX(4);
        }
    });
});

let beginTime = window.performance.now() / 1000;
let frameCount = 0;
let graphs = new graphFactory();

let flatTimeout = undefined;
let showGraph = (graph) => {

    if (existingGraph != undefined) {
        animatables = animatables.filter(animatable => animatable !== existingGraph);
        existingGraph.destroy(scene);
    }

    appSettings.creationStartTime = window.performance.now();

    existingGraph = graph;

    if (scene != undefined)
        graph.init(scene, appSettings);

    animatables.push(graph);
    appSettings.animating = true;
    updateAnimatingControls();
    updateAnimatingButtons();

    appSettings.currentVertexFocus = -1;

    lastRecordedMovement = {};
    animations = {};
    animatingTowardsON = {};
    iteration = 0;

    $("#custom-graph-transitions").val(JSON.stringify(appSettings.graphTransitionFunction));
    if (!appSettings.isSynchronizable)
        M.toast({ html: "Ha! You're out of luck. This automaton is not synchronizable!", displayLength: 3000 });
    if (flatTimeout != undefined)
        clearTimeout(flatTimeout);

    if (stayFlat) {
        flatForce.foo = 0.0;
        toggleFlatForce(true);
    }
    threeDimForce.foo = 0.0;
    togglethreeDimForce(1);
    setMood(appSettings.daytime);
    travelToVertex(-1);
}

let setMood = t => {
    // t is between 0 and 1
    sunLight.color.setHSL(0.1, 1, Math.min(1.0, 1.5 - t));

    skyParameters.distance = 400;
    skyParameters.inclination = -0.3 - t * 0.2;
    skyParameters.azimuth = 0.1 + t * 0.4;
    updateSun();
};
let downloadURI = (uri, name) => {
    var link = document.createElement("a");
    link.download = name;
    link.href = uri;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    delete link;
}
let generatedAlready = false;
let generatePosterShot = () => {
    appSettings.probabilityOfUpdate = 1.0;
    controls.maxDistance = 100000;
    resizeToGoldenRatio();
    // fov
    camera.fov = 35;
    // rotation
    controls.reset();
    // position
    let distance = 1.0 / 2;
    camera.position.set(25 * distance, 1 + 5 * distance, -15 * distance);
    // mood
    setMood(0);
    existingGraph.removeDescription();
    toggleFlatForce(true);
    if (generatedAlready)
        return;
    generatedAlready = true;
    let far = 450;
    let height = water.position.y + 2;

    const description = [
        ["Wysoko wydajne obliczanie słów synchronizujących dla automatów skończonych"],
        ["Michalina Nikonowicz i Kazimierz Wojciechowski"],
        ["Wydział Matematyki i Nauk Informacyjnych"],
        ["Styczeń 2019, Politechnika Warszawska"],
        ["Promotor: dr Michał Dębski"],
        [""],
        ["Obliczenia potwierdzają hipotezę Černego dla n < 11"],
        [""],
        ["Projekt składa się z trzech modułów"],
        ["Klient, Serwer oraz Prezentacja"],
        [""],
        [""],
        [""],
        [""],
        [""],
        [""],
        [""],
        [""],
        [""],
        [""],
        ["SignalR"],
        ["Three.js"],
        ["Materialize"]
    ];

    const size = 14;
    const angle = Math.atan(camera.position.x / camera.position.z);
    let textDistance = 15.5;
    for (let i = 0; i < description.length; i++) {
        const descriptor = description[i];
        if (descriptor == "") {
            textDistance += 0.04 * textDistance;
        } else {
            let text = getTextObjectMatchingWidth(descriptor[0], size, 4, -1);
            text.position.set(camera.position.x + Math.sin(angle) * textDistance, height, camera.position.z + Math.cos(angle) * textDistance);
            textDistance += 0.04 * textDistance + (text.children[0].geometry.boundingBox.max.y - text.children[0].geometry.boundingBox.min.y);
            text.position.y = camera.position.y;

            text.lookAt(camera.position);
            text.position.y = water.position.y;
            scene.add(text);
        }
    }

    water.position.y -= 13;
    height = water.position.y + 1;

    var img = new THREE.MeshBasicMaterial({
        map: THREE.ImageUtils.loadTexture('textures/logo_mini.png')
    });
    img.transparent = true;
    // plane
    const logoSize = 34;
    var plane = new THREE.Mesh(new THREE.PlaneGeometry(logoSize, logoSize), img);
    plane.position.set(camera.position.x + Math.sin(angle) * far, height, camera.position.z + Math.cos(angle) * far);
    plane.position.y = water.position.y + logoSize / 2 + 2;
    plane.overdraw = true;
    plane.renderOrder = 1;
    plane.lookAt(camera.position);
    scene.add(plane);
}
let getTextObjectMatchingWidth = (text, size, align, rotate) => {
    const fontMaterial = new THREE.MeshStandardMaterial({
        color: new THREE.Color(0x666666),
        emissive: new THREE.Color(0x000000),
        roughness: 0.8,
        metalness: 0.5
    });
    const fontHeight = 0.02;
    let fakeFontGeometry = new THREE.TextGeometry(text, {
        font: appSettings.font,
        size: size,
        height: fontHeight,
        curveSegments: 2 + 4 * appSettings.quality,
    });
    fakeFontGeometry.computeBoundingBox();
    const scale = size * size / (fakeFontGeometry.boundingBox.max.x - fakeFontGeometry.boundingBox.min.x);
    let fontGeometry = new THREE.TextGeometry(text, {
        font: appSettings.font,
        size: scale,
        height: fontHeight,
        curveSegments: 2 + 4 * appSettings.quality,
    });
    fontGeometry.computeBoundingBox();
    let textWidth = fontGeometry.boundingBox.max.x - fontGeometry.boundingBox.min.x;
    let group = new THREE.Group();
    let mesh = new THREE.Mesh(fontGeometry, fontMaterial);

    if (align === 1)
        mesh.position.x -= textWidth;
    else if (align !== -1)
        mesh.position.x -= textWidth / 2;
    if (rotate)
        mesh.rotation.x = -Math.PI / 2;
    group.add(mesh);
    return group;
}
$(document.body).css("opacity", 1);
$(document.body).on("keydown", function (e) {});
let takeScreenshot = false;
let touchDist = undefined;
let initialZoom;
$(document.body).on("touchstart", function (e) {
    if (e.touches.length == 2) {
        touchDist = Math.hypot(e.touches[0].pageX - e.touches[1].pageX, e.touches[0].pageY - e.touches[1].pageY);
        initialZoom = cameraDistance;
    }
});
$(document.body).on("touchmove", function (e) {
    if (e.touches.length == 2 && touchDist !== undefined) {
        let newDist = Math.hypot(e.touches[0].pageX - e.touches[1].pageX, e.touches[0].pageY - e.touches[1].pageY);
        let ratio = (touchDist / newDist) ** 2;
        cameraDistance = Math.min(100, initialZoom * ratio);
    }
});