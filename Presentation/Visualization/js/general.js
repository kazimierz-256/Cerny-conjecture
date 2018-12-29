
let stats = new Stats();
let appSettings = new settings(1);

let camera, scene, renderer, controls, mesh, water, composer, outlinePass;
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
            // if (accelerationVector.length() >= ((Math.E * Math.exp(-Math.sqrt((Date.now() - latestActionTimestamp) / timeout))) ** 2) * lengthThreshold) {
            //     if (accelerationVector.length() >= lengthThreshold) {
            //         if (unitAcceleration.z > directionThreshold) {
            //             $("#info").html("zoom in");
            //             cameraDistance /= 1.5;
            //             latestActionTimestamp = Date.now();
            //         } else if (unitAcceleration.z < -directionThreshold) {
            //             $("#info").html("zoom out");
            //             cameraDistance *= 1.5;
            //             latestActionTimestamp = Date.now();

            //         } else {
            //             // $("#info").html("movement");
            //             // moveNow = true;
            //         }
            //     } else {
            //         $("#info").html("too weak");

            //     }
            // }

            // appSettings.accelX = accelerationVector.x;
            // appSettings.accelY = accelerationVector.y;
            // appSettings.accelZ = accelerationVector.z;
            accelerationVector.applyQuaternion(camera.quaternion);

            let ax = accelerationVector.x;
            let ay = accelerationVector.y;
            let az = accelerationVector.z;

            appSettings.accelX = ax;
            appSettings.accelY = ay;
            appSettings.accelZ = az;


            // if (moveNow) {
            // 	moveVector.applyQuaternion(camera.quaternion);
            // 	px += moveVector.x;
            // 	py += moveVector.y;
            // 	pz += moveVector.z;
            // }

            // $("#info").html(
            // 	"avgX: " + ax + "<br>" +
            // 	"avgY: " + ay + "<br>" +
            // 	"avgZ: " + az
            // );
            if (!firstRun) {

                // vx += (ax) * elapsedTime;
                // vy += (ay) * elapsedTime;
                // vz += (az) * elapsedTime;

                // px += vx * elapsedTime;
                // py += vy * elapsedTime;
                // pz += vz * elapsedTime;

                let newCameraPosition =
                    new THREE.Vector3(0, 0, cameraDistance)
                        .applyQuaternion(camera.quaternion);

                camera.position.set(
                    newCameraPosition.x,
                    newCameraPosition.y,
                    newCameraPosition.z
                );

                // helper.position.set(
                //     ax,
                //     ay,
                //     az
                // );

            } else {
                firstRun = false;
            }
            // pvx = vx;
            // pvy = vy;
            // pvz = vz;
            // pax = ax;
            // pay = ay;
            // paz = az;
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

let init = (createControlFromCamera) => {

    renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setPixelRatio(window.devicePixelRatio);
    renderer.setSize(window.innerWidth, window.innerHeight);
    document.body.appendChild(renderer.domElement);

    composer = new THREE.EffectComposer(renderer);
    composer.setSize(window.innerWidth * window.devicePixelRatio, window.innerHeight * window.devicePixelRatio);

    camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.01, 5000);

    appSettings.camera = camera;
    createControlFromCamera(camera);

    scene = new THREE.Scene();

    scene.background = new THREE.Color().setHSL(0.6, 0, 1);
    scene.fog = new THREE.Fog(scene.background, 1, 1000);
    //
    let light = new THREE.DirectionalLight(0xffffff, 1);
    light.color.setHSL(0.1, 1, 0.95);
    // light.position.set(-1, 1.75, 1);
    // light.position.multiplyScalar(30);
    scene.add(light);

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
            sunDirection: light.position.clone().normalize(),
            sunColor: 0xffffff,
            waterColor: 0x001e0f,
            distortionScale: 1.5,
            fog: scene.fog !== undefined
        }
    );
    water.material.uniforms.size.value *= 4;
    water.rotation.x = - Math.PI / 2;
    water.position.y = -10;
    scene.add(water);

    // TODO: remove
    let groundGeo = new THREE.PlaneBufferGeometry(10000, 10000);
    let groundMat = new THREE.MeshPhongMaterial({ color: 0xffffff });
    groundMat.color.setHSL(0.6, 1, 1);
    let ground = new THREE.Mesh(groundGeo, groundMat);
    ground.rotation.x = -Math.PI / 2;
    ground.position.y = -33;
    // scene.add(ground);

    var sky = new THREE.Sky();
    sky.scale.setScalar(10000);
    scene.add(sky);
    var uniforms = sky.material.uniforms;
    uniforms.turbidity.value = 10;
    uniforms.rayleigh.value = 1;
    uniforms.luminance.value = 1;
    uniforms.mieCoefficient.value = 0.005;
    uniforms.mieDirectionalG.value = 0.8;
    var parameters = {
        distance: 400,
        inclination: 0.3,
        azimuth: 0.1
    };
    var cubeCamera = new THREE.CubeCamera(1, 20000, 256);
    cubeCamera.renderTarget.texture.generateMipmaps = true;
    cubeCamera.renderTarget.texture.minFilter = THREE.LinearMipMapLinearFilter;
    function updateSun() {
        var theta = Math.PI * (parameters.inclination - 0.5);
        var phi = 2 * Math.PI * (parameters.azimuth - 0.5);
        light.position.x = parameters.distance * Math.cos(phi);
        light.position.y = parameters.distance * Math.sin(phi) * Math.sin(theta);
        light.position.z = parameters.distance * Math.sin(phi) * Math.cos(theta);
        sky.material.uniforms.sunPosition.value = light.position.copy(light.position);
        water.material.uniforms.sunDirection.value.copy(light.position).normalize();
        cubeCamera.update(renderer, scene);
    }
    updateSun();


    // let renderPass = new THREE.RenderPass(scene, camera);
    // composer.addPass(renderPass);

    // outlinePass = new THREE.OutlinePass(new THREE.Vector2(window.innerWidth * window.devicePixelRatio, window.innerHeight * window.devicePixelRatio), scene, camera);

    // outlinePass.edgeStrength = 10.0;
    // outlinePass.edgeGlow = 0.0;
    // outlinePass.edgeThickness = 1.0;
    // outlinePass.pulsePeriod = 0;
    // outlinePass.rotate = false;
    // outlinePass.usePatternTexture = false;
    // outlinePass.selectedObjects = [];
    // outlinePass.renderToScreen = true;
    // outlinePass.visibleEdgeColor.set("#0x000000");

    // composer.addPass(outlinePass);

    // var gui = new dat.GUI();
    // var folder = gui.addFolder('Sky');
    // folder.add(parameters, 'inclination', 0, 0.5, 0.0001).onChange(updateSun);
    // folder.add(parameters, 'azimuth', 0, 1, 0.0001).onChange(updateSun);
    // folder.open();

    // let vertexShader = document.getElementById('vertexShader').textContent;
    // let fragmentShader = document.getElementById('fragmentShader').textContent;

    // let plainSkUniforms = {
    //     topColor: { value: new THREE.Color("hsl(210, 80%, 50%)") },
    //     bottomColor: { value: groundMat.color },
    //     offset: { value: 33 },
    //     exponent: { value: 1 }
    // };

    // let hemiLight = new THREE.HemisphereLight(0xffffff, 0xffffff, 1.2);
    // hemiLight.color.copy(plainSkUniforms.topColor.value);
    // hemiLight.groundColor.copy(groundMat.color);
    // hemiLight.position.set(0, 50, 0);
    // scene.add(hemiLight);

    // plainSkUniforms.topColor.value.copy(hemiLight.color);
    // scene.fog.color.copy(plainSkUniforms.bottomColor.value);
    // let skyGeo = new THREE.SphereBufferGeometry(4000, 32, 15);
    // let skyMat = new THREE.ShaderMaterial({ vertexShader: vertexShader, fragmentShader: fragmentShader, uniforms: plainSkUniforms, side: THREE.BackSide });
    // let sky = new THREE.Mesh(skyGeo, skyMat);
    // scene.add(sky);

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
                travelToVertex(appSettings.shortestPath[i], "easeInOutExpo", 1500, animateFurther[i - 1]);
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
    $("#zoomin").click((e) => {
        dolly(cameraDistance / zoomFactor);
        e.stopPropagation();
    });
    $("#zoomout").click((e) => {
        dolly(cameraDistance * zoomFactor);
        e.stopPropagation();
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

    let travelToVertex = (toVertex, easing, duration, onComplete) => {
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
            M.toast({ html: "The automaton is now focusing at vertex " + (mouseDownConsideringVertex >>> 0).toString(2).padStart(appSettings.n, "0"), displayLength: 1000 })
        }
    });

    //further part

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
        showGraph(graphs.getRandomAutomaton($("#random-graph-size").val(), appSettings, outlinePass));
    });
    $("#cerny-graph-generate").on("click", () => {
        showGraph(graphs.getCernyAutomaton($("#cerny-graph-size").val(), appSettings, outlinePass));
    });
    $("#custom-graph-generate").on("click", () => {
        showGraph(parseGraph($("#custom-graph-transitions").val(), outlinePass));
    });
    $("#generate-karis-automaton").on("click", () => {
        showGraph(graphs.getKarisAutomaton(appSettings, outlinePass));
    });
    $("#generate-extreme-3-automaton").on("click", () => {
        showGraph(graphs.getExtreme3Automaton(appSettings, outlinePass));
    });
    $("#generate-extreme-4-automaton").on("click", () => {
        showGraph(graphs.getExtreme4Automaton(appSettings, outlinePass));
    });

    $("#quality-ultra").on("click", () => {
        appSettings.quality = 3;
        M.toast({ html: "Ultra quality setting will be applied once a new automaton is created", displayLength: 4000 })
    });

    $("#quality-standard").on("click", () => {
        appSettings.quality = 2;
        M.toast({ html: "Standard quality setting will be applied once a new automaton is created", displayLength: 4000 })
    });

    $("#quality-low").on("click", () => {
        appSettings.quality = 1;
        M.toast({ html: "Low quality setting will be applied once a new automaton is created", displayLength: 4000 })
    });

    $("#quality-minimum").on("click", () => {
        appSettings.quality = 0;
        M.toast({ html: "Minimum quality setting will be applied once a new automaton is created", displayLength: 4000 })
    });

    $("#automaton-lights").change((e) => {
        appSettings.shineLights = e.target.checked;
        if (appSettings.shineLights)
            M.toast({ html: "Lights will be available once a new automaton is created", displayLength: 4000 })
    });

    $("#repelling-edges").change((e) => {
        appSettings.repelArrows = e.target.checked;
        if (appSettings.repelArrows)
            M.toast({ html: "Be careful! This feature is experimental. Your graph may blow up.", displayLength: 6000 })
    });

    $("#automaton-speedup").change((e) => {
        appSettings.speedup = parseFloat(e.target.options[e.target.selectedIndex].value);
    });
    $("#automaton-repel").change((e) => {
        appSettings.repellingConstant = e.target.value * appSettings.forceStrength;
    });
    $("#automaton-string").change((e) => {
        appSettings.stringConstant = e.target.value * appSettings.forceStrength;
    });
    $("#automaton-unique-string").change((e) => {
        appSettings.uniqueStringConstant = e.target.value * appSettings.stringConstant;
    });
    $("#automaton-friction").change((e) => {
        appSettings.friction = e.target.value * appSettings.forceStrength;
    });
    $("#string-length").change((e) => {
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
        M.toast({ html: "The automaton is now focusing at the center of mass", displayLength: 1000 });
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

    // if (blurClearTimeout != undefined)
    //     clearTimeout(blurClearTimeout);

    // let toBlur = appSettings.smallBlur;
    // if (appSettings.quality <= 1) {
    //     makeBlurred = false;
    // }
    // if (makeBlurred == undefined) {
    //     makeBlurred = !previouslyBlurred;
    // } else if (makeBlurred) {
    //     toBlur = (blurAmount == undefined) ? appSettings.maxBlur : blurAmount;
    // } else {
    //     toBlur = 0;
    // }

    // $(blurAnimation).stop(true, true);
    // $(blurAnimation).animate({ foo: toBlur }, {
    //     duration: (timeout == undefined) ? appSettings.blurTimeout : timeout,
    //     easing: appSettings.smoothOutEasing,
    //     step: (now) => {
    //         $("body > canvas").css({ "filter": "blur(" + now + "px)" });
    //     },
    //     complete: () => {
    //         if (toBlur == 0) {
    //             $("body > canvas").css("filter", "none");
    //         }
    //     }
    // });

    // previousBlur = toBlur;
};

let parseGraph = (specification) => {
    let parsedGraph = JSON.parse(specification);
    console.log(parsedGraph);
    return getAnimatableGraph(parsedGraph, appSettings, "User's custom automaton of size " + Math.min(parsedGraph[0].length, parsedGraph[1].length), outlinePass);
};

let animate = () => {
    stats.end();
    stats.begin();
    let time = Date.now() / 1000 - beginTime;
    water.material.uniforms.time.value += 1.0 / 60.0;
    animatables.forEach(animatable => animatable.update(time, appSettings));
    controls.update();
    renderer.render(scene, camera);
    // composer.render();
    setTimeout(() => window.requestAnimationFrame(animate), 0);
}

let onWindowResize = () => {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
    composer.setSize(window.innerWidth, window.innerHeight);
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
            if (request["a"] == undefined) {
                showGraph(graphs.getCernyAutomaton(4, appSettings, outlinePass));
            } else {
                showGraph(parseGraph(unescape(request["a"])));
            }
        };

        if ('LinearAccelerationSensor' in window && ('ontouchstart' in window)) {
            navigator.permissions.query({ name: "accelerometer" }).then(result => {
                if (result.state != 'granted') {
                    document.title = "Sorry, we're not allowed to access sensors " +
                        "on your device..";
                    return;
                }
                init(camera => controls = new THREE.DeviceOrientationControls(camera));
                // init(camera => {
                //     controls = new THREE.OrbitControls(camera);
                // });
                // for mobile devices
                if (!('ontouchstart' in window)) {
                    camera.position.setY(2);
                    camera.position.setX(4);
                } else {
                    // $(".btn-floating").addClass("btn-small");
                }
                showGraphFromRequest();
                animate();
                startProcessingSensorData();
            });
        } else {
            // probably desktop
            // document.title = "Your browser doesn't support sensors.";
            init(camera => {
                controls = new THREE.OrbitControls(camera);
                controls.enableDamping = true;
                controls.dampingFactor = 0.3;
                controls.maxPolarAngle = Math.PI * 0.495;//Math.PI * 4 / 5;
                controls.minPolarAngle = Math.PI * 1 / 5;
                controls.minDistance = 0.7;
                // controls.enablePan = false;
                controls.zoomSpeed = 4;
            });
            if (!('ontouchstart' in window)) {
                camera.position.setY(2);
                camera.position.setX(4);
            } else {
                // $(".btn-floating").addClass("btn-small");
            }
            $("#zoomin").remove();
            $("#zoomout").remove();
            showGraphFromRequest();
            animate();
            // $(window).blur(() => {
            //     doAnimate = false;
            //     document.title = "stopped";
            // });
            // for mobile devices
        }

        // for mobile devices
        if (!('ontouchstart' in window)) {
            camera.position.setY(2);
            camera.position.setX(4);
        } else {
            // $(".btn-floating").addClass("btn-small");
        }
    });
});

let beginTime = Date.now() / 1000;
let graphs = new graphFactory();
let existingGraph = undefined;

let flatTimeout = undefined;
let showGraph = (graph) => {

    if (existingGraph != undefined) {
        animatables = animatables.filter(animatable => animatable !== existingGraph);
        existingGraph.destroy();
    }

    appSettings.creationStartTime = Date.now();

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
        M.toast({ html: "Ha! You're out of luck. This automaton is not synchronizable!", displayLength: 10000 });
    if (flatTimeout != undefined)
        clearTimeout(flatTimeout);
    flatTimeout = setTimeout(() => {
        M.toast({ html: "This graph is now entirely in three dimensions! Other coordinates are now zeroes.", displayLength: 10000 });
    }, appSettings.flatteningForceTimeout);

    if (stayFlat) {
        flatForce.foo = 0.0;
        toggleFlatForce(true);
    }
    threeDimForce.foo = 0.0;
    togglethreeDimForce(1);

    travelToVertex(-1);
}

$(document.body).css("opacity", 1);