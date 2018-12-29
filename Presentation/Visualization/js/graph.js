function getAnimatableGraph(problem, appSettings, graphDescription) {
    appSettings.graphTransitionFunction = problem;

    let n = Math.min(problem[0].length, problem[1].length);
    appSettings.n = n;
    let power = 1 << n;

    let isDiscovered = new Array(power).fill(false);
    let isChargeActive = new Array(power).fill(false);
    let wentByB = new Array(power).fill(false);
    let partOfShortestWord = new Array(power).fill(false);

    let vertices = new Int16Array(power);
    let vertexDistance = new Int16Array(power);
    let vertexHighlightedBits = new Int16Array(power);
    let preceedingVertex = new Int16Array(power);
    let succeedingVertex = new Int16Array(power);
    let missingBits = new Int16Array(power);

    let connectionA = new Int16Array(power);
    let connectionB = new Int16Array(power);

    let discoveredQueue = new Int16Array(power);
    let readingIndex = 0;
    let writingIndex = 1;

    let transitionA = new Int16Array(n);
    let transitionB = new Int16Array(n);

    let startingVertex = power - 1;
    discoveredQueue[0] = startingVertex;
    vertexDistance[startingVertex] = 1;
    isDiscovered[startingVertex] = true;
    preceedingVertex[startingVertex] = startingVertex;
    let discoveredSingleton = false;
    let singletonVertex = -1;

    for (let i = 0; i < n; i++) {
        if (Number.isInteger(problem[0][i]) && problem[0][i] < n)
            transitionA[i] = 1 << problem[0][i];
        if (Number.isInteger(problem[1][i]) && problem[1][i] < n)
            transitionB[i] = 1 << problem[1][i];
    }

    let maxDistance = 1;
    while (readingIndex < writingIndex) {
        let consideringVertex = discoveredQueue[readingIndex++];

        if (vertexDistance[consideringVertex] > maxDistance)
            maxDistance = vertexDistance[consideringVertex];

        let goA = 0;
        let goB = 0;
        let missingA = missingBits[consideringVertex];
        let missingB = missingBits[consideringVertex];
        let bitsCons = 0;
        for (let i = 0; i < n; i++) {
            if (((consideringVertex >>> i) & 1) === 1) {
                goA |= transitionA[i];
                goB |= transitionB[i];

                if (!Number.isInteger(problem[0][i]) || problem[0][i] >= n)
                    missingA += 1;
                if (!Number.isInteger(problem[1][i]) || problem[1][i] >= n)
                    missingB += 1;
                bitsCons++;
            } else {
                // bit not set
            }
        }

        connectionA[consideringVertex] = goA;
        connectionB[consideringVertex] = goB;

        vertexHighlightedBits[consideringVertex] = bitsCons;
        // check whether they form a new vertex
        if (isDiscovered[goA] === false) {
            missingBits[goA] = missingA;
            if (!discoveredSingleton) {
                if (0 == (goA & (goA - 1)) && missingA == 0) {
                    discoveredSingleton = true;
                    singletonVertex = goA;
                }
            }
            discoveredQueue[writingIndex++] = goA;
            vertexDistance[goA] = vertexDistance[consideringVertex] + 1;
            isDiscovered[goA] = true;
            preceedingVertex[goA] = consideringVertex;
            wentByB[goA] = false;
        }
        if (isDiscovered[goB] === false) {
            missingBits[goB] = missingB;
            if (!discoveredSingleton) {
                if (0 == (goB & (goB - 1)) && missingB == 0) {
                    discoveredSingleton = true;
                    singletonVertex = goB;
                }
            }
            discoveredQueue[writingIndex++] = goB;
            vertexDistance[goB] = vertexDistance[consideringVertex] + 1;
            isDiscovered[goB] = true;
            preceedingVertex[goB] = consideringVertex;
            wentByB[goA] = true;
        }
    }
    appSettings.maxDistance = maxDistance;
    appSettings.maximumConsideringDepth = maxDistance;

    appSettings.shortestPath = undefined;
    if (discoveredSingleton) {
        appSettings.shortestPath = [];
        partOfShortestWord[startingVertex] = true;
        let elevatingVertex = singletonVertex;
        while (true) {
            appSettings.shortestPath.push(elevatingVertex);
            partOfShortestWord[elevatingVertex] = true;
            let newElevatingVertex = preceedingVertex[elevatingVertex];
            succeedingVertex[newElevatingVertex] = elevatingVertex;
            if (newElevatingVertex == startingVertex) {
                appSettings.shortestPath.push(startingVertex);
                break;
            } else {
                elevatingVertex = newElevatingVertex;
            }
        }
    }

    appSettings.isSynchronizable = discoveredSingleton;

    let vx = new Float32Array(power);
    let vy = new Float32Array(power);
    let vz = new Float32Array(power);
    let vw = new Float32Array(power);
    let vu = new Float32Array(power);

    let px = new Float32Array(power);
    let py = new Float32Array(power);
    let pz = new Float32Array(power);
    let pw = new Float32Array(power);
    let pu = new Float32Array(power);

    let FxArr = new Float32Array(power);
    let FyArr = new Float32Array(power);
    let FzArr = new Float32Array(power);
    let FwArr = new Float32Array(power);
    let FuArr = new Float32Array(power);

    appSettings.recomputeForces(10 / power);

    let sphereSize = 1;
    let distanceToSize = (dist) => sphereSize / Math.sqrt(dist);
    sphereSize /= distanceToSize(1);
    sphereSize /= 3;

    let spheres = new Array(power);
    let vertexDetails = new Array(power);
    let sphereGroup = new Array(power);
    let lights = new Array(power);
    let arrows = new Array(power * 2);
    let arrowTransitions = new Array(power * 2);
    let arrowTransitionWidth = new Array(power * 2);
    let mass = new Float32Array(power);
    let graph = new THREE.Group();
    let descriptionFontMesh, descriptionFontMesh2;

    if (graphDescription != undefined) {
        let fontSize = 1;
        let fontGeometry = new THREE.TextGeometry(graphDescription, {
            font: appSettings.font,
            size: fontSize,
            height: fontSize * 0.1,
            curveSegments: 2 + 2 * appSettings.quality,
        });
        let fontGeometry2 = new THREE.TextGeometry(JSON.stringify(problem), {
            font: appSettings.font,
            size: fontSize,
            height: fontSize * 0.1,
            curveSegments: 2 + 2 * appSettings.quality,
        });
        let fontMaterial = new THREE.MeshStandardMaterial({
            color: new THREE.Color(0xffffff),
            roughness: 0.5,
            metalness: 0
        });
        fontGeometry.computeBoundingBox();
        fontGeometry2.computeBoundingBox();
        let textWidth = fontGeometry.boundingBox.max.x - fontGeometry.boundingBox.min.x;
        let textWidth2 = fontGeometry2.boundingBox.max.x - fontGeometry2.boundingBox.min.x;

        descriptionFontMesh = new THREE.Mesh(fontGeometry, fontMaterial);
        descriptionFontMesh.position.y = 1;
        descriptionFontMesh.position.x = -10;
        descriptionFontMesh.position.z = textWidth / 2;
        descriptionFontMesh.rotation.y = Math.PI / 2;

        descriptionFontMesh2 = new THREE.Mesh(fontGeometry2, fontMaterial);
        descriptionFontMesh2.position.y = -1;
        descriptionFontMesh2.position.x = -10;
        descriptionFontMesh2.position.z = textWidth2 / 2;
        descriptionFontMesh2.rotation.y = Math.PI / 2;

        graph.add(descriptionFontMesh);
        graph.add(descriptionFontMesh2);
    }

    for (let i = 0; i < power; i++) {
        if (!isDiscovered[i])
            continue;

        mass[i] = distanceToSize(vertexDistance[i]);
        let geometry = new THREE.SphereGeometry(mass[i], 4 + appSettings.quality * 10, 4 + appSettings.quality * 10);

        let distanceGeneralIntensity = (vertexDistance[i] - 1) / vertexDistance[singletonVertex];
        let highlightedBitsIntensity = (1 - (vertexHighlightedBits[i] - 1) / (n - 1));
        let color = new THREE.Color();
        let hue = 0;
        let saturation = 0.0;
        let lighting = 0.0;
        if (partOfShortestWord[i] === true) {
            hue = 0.25 + (0.25 * highlightedBitsIntensity);
            saturation = 0.6;
            lighting = 0.6;
        } else {
            lighting = 0.4;
        }
        color.setHSL(hue, saturation, lighting);

        let material = new THREE.MeshStandardMaterial({
            color: color,
            roughness: 0.65,
            metalness: 0.4
        });
        sphereGroup[i] = new THREE.Group();

        if (partOfShortestWord[i] === true) {
            material.emissive.copy(color);
            material.emissive.offsetHSL(0, 0, 0.2 * distanceGeneralIntensity);
            if (appSettings.shineLights && appSettings.quality >= 1) {
                lights[i] = new THREE.PointLight(color, 0.15 * (1 - distanceGeneralIntensity), 50);
                sphereGroup[i].add(lights[i]);
            }
        }

        spheres[i] = new THREE.Mesh(geometry, material);
        // spheres[i].castShadow = true;
        // spheres[i].receiveShadow = true;
        spheres[i].automatonId = i;
        spheres[i].material.defaultColor = material.color;
        sphereGroup[i].add(spheres[i]);

        let textTosShow = (i >>> 0).toString(2).padStart(n, "0") + (missingBits[i] > 0 ? " (" + missingBits[i] + ")" : "");
        let fontSize = mass[i] * 1.9 / textTosShow.length;
        let fontGeometry = new THREE.TextGeometry(
            textTosShow, {
                font: appSettings.font,
                size: fontSize,
                height: fontSize * 0.1,
                curveSegments: 1 + appSettings.quality,
            });
        let fontMaterial = new THREE.MeshStandardMaterial({
            color: new THREE.Color(partOfShortestWord[i] ? 0x000000 : 0xffffff),
            emissive: new THREE.Color(partOfShortestWord[i] ? 0x000000 : 0xffffff),
            roughness: 0,
            metalness: 1
        });
        fontGeometry.computeBoundingBox();
        let textWidth = fontGeometry.boundingBox.max.x - fontGeometry.boundingBox.min.x;
        let textHeight = fontGeometry.boundingBox.max.y - fontGeometry.boundingBox.min.y;


        let fontMesh = new THREE.Mesh(fontGeometry, fontMaterial);
        fontMesh.position.y = -textHeight / 2;
        fontMesh.position.x = -textWidth / 2;
        fontMesh.position.z = mass[i] * 1;

        vertexDetails[i] = new THREE.Group();
        vertexDetails[i].add(fontMesh);
        vertexDetails[i].position.set(
            0,
            0,
            0
        );
        sphereGroup[i].add(vertexDetails[i]);

        px[i] = 1 - 2 * (Math.random() * vertexDistance[i]) / maxDistance;
        py[i] = 1 - 2 * (Math.random() * vertexDistance[i]) / maxDistance;
        pz[i] = 1 - 2 * (Math.random() * vertexDistance[i]) / maxDistance;
        pw[i] = 1 - 2 * (Math.random() * vertexDistance[i]) / maxDistance;
        pu[i] = 1 - 2 * (Math.random() * vertexDistance[i]) / maxDistance;
        vx[i] = px[i];
        vy[i] = py[i];
        vz[i] = pz[i];
        vw[i] = pw[i];
        vu[i] = pu[i];
    }


    for (let i = 0; i < power; i++) {
        if (!isDiscovered[i])
            continue;
        for (let index = 0; index < 2; index++) {
            let j = index == 0 ? connectionA[i] : connectionB[i];
            if (i == j)
                continue;

            let partOfShortestPath = (partOfShortestWord[i] && partOfShortestWord[j] && preceedingVertex[j] == i);
            // let color = new THREE.Color(0x222222);
            // let hue = index == 0 ? 0.6 : 0.0;
            // let saturation = 0;
            // let lighting = partOfShortestPath ? 0.5 : 0;
            // color.setHSL(hue, saturation, lighting);
            // if (partOfShortestPath) {
            //     color = spheres[i].material.emissive;
            //     // TODO: make it shinier???
            // }

            let arrow = new THREE.ArrowHelper(
                spheres[j].position.clone().sub(spheres[i].position).normalize(),
                spheres[i].position,
                1,
                0x222222,
                0.2,
                0.04,
                5 + 7 * appSettings.quality
            );

            if (!partOfShortestPath) {
                arrow.cone.material.transparent = true;
                arrow.line.material.transparent = true;
                arrow.cone.material.opacity = 0.1;
                arrow.line.material.opacity = 0.1;
                arrow.line.material.linewidth = 1;
            } else {
                arrow.line.material.linewidth = 2;
            }

            graph.add(arrow);
            arrows[2 * i + index] = arrow;
            if (connectionA[i] != connectionB[i] || index == 0) {
                let fontSize = mass[i] * 0.4;
                let fontText = index == 0 ? "a" : "b";
                if (connectionA[i] == connectionB[i]) {
                    fontText = "ab";
                }
                let fontGeometry = new THREE.TextGeometry(fontText, {
                    font: appSettings.font,
                    size: fontSize,
                    height: fontSize * 0.1,
                    curveSegments: 1 + appSettings.quality,
                });
                let fontMaterial = new THREE.MeshStandardMaterial({
                    color: new THREE.Color(0x000000),
                    roughness: 0,
                    metalness: 0,
                    opacity: (partOfShortestPath ? 1 : 0.2),
                    transparent: true
                });
                fontGeometry.computeBoundingBox();
                let textWidth = fontGeometry.boundingBox.max.x - fontGeometry.boundingBox.min.x;
                fontGeometry.translate(0, 0.04, 0);
                let fontMesh = new THREE.Mesh(fontGeometry, fontMaterial);

                graph.add(fontMesh);
                arrowTransitions[2 * i + index] = fontMesh;
                arrowTransitionWidth[2 * i + index] = textWidth;
            }
        }
    }

    for (let i = 0; i < power; i++) {
        if (!isDiscovered[i])
            continue;
        graph.add(sphereGroup[i]);
    }

    let init = scene => scene.add(graph);
    let destroy = (t, appSettings) => {
        scene.remove(graph);
    };
    let latestTime = 1.0;

    let update = (t, appSettings) => {
        if (appSettings.animating) {
            let deltaT = (t - latestTime) * appSettings.speedup / appSettings.deltaTSlowdown;
            let maxDeltaT = 0.05 * appSettings.speedup;
            if (latestTime < 0.0) {
                deltaT = 0.0;
            } else if (Math.abs(deltaT) > maxDeltaT) {
                deltaT = maxDeltaT;
            }
            if (appSettings.maxSpeedSquared > appSettings.maxPermissibleSpeedSquared) {
                appSettings.deltaTSlowdown *= 2;

            } else if (appSettings.deltaTSlowdown > 1) {
                appSettings.deltaTSlowdown /= 2;
            }

            latestTime = t;

            // make the central vertex attracted by the point [0,0]
            // firstly, make the arrows repell themselves
            // then make the spheres react to this

            let sumPx = 0.0;
            let sumPy = 0.0;
            let sumPz = 0.0;
            let sumPw = 0.0;
            let sumPu = 0.0;
            let maxSpeedSquared = 0;

            let totalMass = 0.0;
            let elapsedSeconds = 0.0;
            for (let i = 0; i < power; i++) {
                if (!isDiscovered[i] || vertexDistance[i] > appSettings.maximumConsideringDepth)
                    continue;
                FxArr[i] = appSettings.accelX * -10 - vx[i] * appSettings.friction;
                FyArr[i] = appSettings.accelY * -10 - vy[i] * appSettings.friction;
                FzArr[i] = appSettings.accelZ * -10 - vz[i] * appSettings.friction;
                FwArr[i] = appSettings.threeDimForceFraction == 1.0 ? 0.0 : (- vw[i] * appSettings.friction);
                FuArr[i] = appSettings.threeDimForceFraction == 1.0 ? 0.0 : (- vu[i] * appSettings.friction);
            }

            for (let i = 0; i < power; i++) {
                if (!isDiscovered[i] || vertexDistance[i] > appSettings.maximumConsideringDepth)
                    continue;

                let massI = mass[i];

                if (appSettings.flatteningForceFraction > 0.0 && Math.abs(py[i]) > appSettings.eps) {
                    FyArr[i] -= appSettings.planarityConstant * py[i] * appSettings.flatteningForceFraction;
                }
                if (appSettings.threeDimForceFraction > 0.0) {
                    FwArr[i] -= appSettings.planarityConstant * pw[i] * appSettings.threeDimForceFraction;
                    FuArr[i] -= appSettings.planarityConstant * pu[i] * appSettings.threeDimForceFraction;
                }
                for (let j = 0; j < i; j++) {
                    if (!isDiscovered[j] || vertexDistance[j] > appSettings.maximumConsideringDepth) {
                        continue;
                    }
                    let massJtimesMassI = mass[j] * massI;

                    let rx = px[j] - px[i];
                    let ry = appSettings.flatteningForceFraction == 1.0 ? 0.0 : (py[j] - py[i]);
                    let rz = pz[j] - pz[i];
                    let rw = appSettings.threeDimForceFraction == 1.0 ? 0.0 : (pw[j] - pw[i]);
                    let ru = appSettings.threeDimForceFraction == 1.0 ? 0.0 : (pu[j] - pu[i]);

                    let r = Math.sqrt(rx * rx + ry * ry + rz * rz + rw * rw + ru * ru);
                    let r2 = r * r;
                    let r3 = r2 * r;

                    if (r3 < appSettings.eps)
                        r3 = appSettings.eps;
                    // compute acceleration based on charge
                    elapsedSeconds = appSettings.repellingConstant * massJtimesMassI / r3;
                    FxArr[i] += elapsedSeconds * rx;
                    FyArr[i] += elapsedSeconds * ry;
                    FzArr[i] += elapsedSeconds * rz;

                    FxArr[j] -= elapsedSeconds * rx;
                    FyArr[j] -= elapsedSeconds * ry;
                    FzArr[j] -= elapsedSeconds * rz;
                    if (appSettings.threeDimForceFraction < 1.0) {
                        FwArr[i] += elapsedSeconds * rw;
                        FuArr[i] += elapsedSeconds * ru;

                        FwArr[j] -= elapsedSeconds * rw;
                        FuArr[j] -= elapsedSeconds * ru;
                    }

                    let appropriateStringConstant = 0;
                    if (connectionA[i] == j
                        || connectionB[i] == j
                        || connectionA[j] == i
                        || connectionB[j] == i) {

                        if (partOfShortestWord[i] && partOfShortestWord[j] && (preceedingVertex[j] == i || preceedingVertex[i] == j)) {
                            appropriateStringConstant = appSettings.uniqueStringConstant;
                        } else {
                            appropriateStringConstant = appSettings.stringConstant;
                        }
                    }

                    // for each outgoing arrow of i
                    // for each outgoiing arrow of j
                    // copute force exerted on arrow i and assign it to vertex i
                    // use the same force to exert on target vertex of arrow
                    let lengthDifferenceFraction = (r - appSettings.targetStringLength) / r;

                    FxArr[i] += appropriateStringConstant * rx * lengthDifferenceFraction;
                    FyArr[i] += appropriateStringConstant * ry * lengthDifferenceFraction;
                    FzArr[i] += appropriateStringConstant * rz * lengthDifferenceFraction;

                    FxArr[j] -= appropriateStringConstant * rx * lengthDifferenceFraction;
                    FyArr[j] -= appropriateStringConstant * ry * lengthDifferenceFraction;
                    FzArr[j] -= appropriateStringConstant * rz * lengthDifferenceFraction;

                    if (appSettings.threeDimForceFraction < 1.0) {
                        FwArr[i] += appropriateStringConstant * rw * lengthDifferenceFraction;
                        FuArr[i] += appropriateStringConstant * ru * lengthDifferenceFraction;

                        FwArr[j] -= appropriateStringConstant * rw * lengthDifferenceFraction;
                        FuArr[j] -= appropriateStringConstant * ru * lengthDifferenceFraction;
                    }

                }
            }

            // compute repelling forces coming from edges!
            if (appSettings.repelArrows)
                for (let i = 0; i < power; i++) {
                    if (!isDiscovered[i] || vertexDistance[i] > appSettings.maximumConsideringDepth)
                        continue;
                    for (let indexI = 0; indexI < 2; indexI++) {
                        let arrowIindexTo = indexI == 0 ? connectionA[i] : connectionB[i];

                        for (let j = 0; j < power; j++) {
                            if (!isDiscovered[j] || vertexDistance[j] > appSettings.maximumConsideringDepth)
                                continue;

                            for (let indexJ = 0; indexJ < 2; indexJ++) {
                                let arrowJindexTo = indexJ == 0 ? connectionA[j] : connectionB[j];

                                let rx = (px[indexI] + px[arrowIindexTo]) / 2 - (px[indexJ] + px[arrowJindexTo]) / 2;
                                let ry = (py[indexI] + py[arrowIindexTo]) / 2 - (py[indexJ] + py[arrowJindexTo]) / 2;
                                let rz = (pz[indexI] + pz[arrowIindexTo]) / 2 - (pz[indexJ] + pz[arrowJindexTo]) / 2;
                                let rw = (pw[indexI] + pw[arrowIindexTo]) / 2 - (pw[indexJ] + pw[arrowJindexTo]) / 2;
                                let ru = (pu[indexI] + pu[arrowIindexTo]) / 2 - (pu[indexJ] + pu[arrowJindexTo]) / 2;
                                let r = Math.sqrt(rx * rx + ry * ry + rz * rz + rw * rw + ru * ru);
                                if (r <= appSettings.eps)
                                    continue;

                                let r3 = r * r * r;

                                let tmp = appSettings.arrowRepellingConstant / r3;
                                FxArr[i] += tmp * rx;
                                FyArr[i] += tmp * ry;
                                FzArr[i] += tmp * rz;
                                FwArr[i] += tmp * rw;
                                FuArr[i] += tmp * ru;

                            }
                        }
                    }
                }

            for (let i = 0; i < power; i++) {
                if (!isDiscovered[i] || vertexDistance[i] > appSettings.maximumConsideringDepth)
                    continue;

                // recompute velocity

                let massI = mass[i];
                elapsedSeconds = deltaT / massI;
                let tmpVx = FxArr[i] * elapsedSeconds;
                let tmpVy = FyArr[i] * elapsedSeconds;
                let tmpVz = FzArr[i] * elapsedSeconds;
                let tmpVw = FwArr[i] * elapsedSeconds;
                let tmpVu = FuArr[i] * elapsedSeconds;

                vx[i] += tmpVx;
                vy[i] += tmpVy;
                vz[i] += tmpVz;
                if (appSettings.threeDimForceFraction < 1.0) {
                    vw[i] += tmpVw;
                    vu[i] += tmpVu;
                }
                maxSpeedSquared = Math.max(
                    maxSpeedSquared,
                    tmpVx * tmpVx +
                    tmpVy * tmpVy +
                    tmpVz * tmpVz +
                    tmpVw * tmpVw +
                    tmpVu * tmpVu
                );

                // recompute position
                px[i] += vx[i] * deltaT;
                py[i] += vy[i] * deltaT;
                pz[i] += vz[i] * deltaT;
                if (appSettings.threeDimForceFraction < 1.0) {
                    pw[i] += vw[i] * deltaT;
                    pu[i] += vu[i] * deltaT;
                }


            }


            for (let i = 0; i < power; i++) {
                let vertexDistLocal = vertexDistance[i];
                let maxOfDepth = Math.max(appSettings.maximumConsideringDepth, appSettings.previousMaximumConsideringDepth);
                if (!isDiscovered[i] || vertexDistLocal > maxOfDepth)
                    continue;
                let massI = mass[i];
                let fraction = 1.0;;
                let minOfDepth = Math.min(appSettings.maximumConsideringDepth, appSettings.previousMaximumConsideringDepth);
                if (vertexDistLocal > minOfDepth && vertexDistLocal <= maxOfDepth) {
                    if (appSettings.maximumConsideringDepth > appSettings.previousMaximumConsideringDepth) {
                        fraction = appSettings.distanceFocusFraction;
                    } else {
                        fraction = 1.0 - appSettings.distanceFocusFraction;
                    }
                    massI *= fraction;
                }
                // to center out the graph in 3d
                sumPx += px[i] * massI;
                sumPy += py[i] * massI;
                sumPz += pz[i] * massI;
                sumPw += pw[i] * massI;
                sumPu += pu[i] * massI;

                totalMass += massI;

            }
            appSettings.maxSpeedSquared = maxSpeedSquared;

            const arrowOffset = 0.1;

            let sourceCenteringX = sumPx / totalMass;
            let sourceCenteringY = sumPy / totalMass;
            let sourceCenteringZ = sumPz / totalMass;
            let sourceCenteringW = sumPw / totalMass;
            let sourceCenteringU = sumPu / totalMass;

            let targetCenteringX = sourceCenteringX;
            let targetCenteringY = sourceCenteringY;
            let targetCenteringZ = sourceCenteringZ;
            let targetCenteringW = sourceCenteringW;
            let targetCenteringU = sourceCenteringU;

            if (appSettings.previousVertexFocus >= 0 && vertexDistance[appSettings.previousVertexFocus] <= appSettings.maximumConsideringDepth) {
                sourceCenteringX = px[appSettings.previousVertexFocus];
                sourceCenteringY = py[appSettings.previousVertexFocus];
                sourceCenteringZ = pz[appSettings.previousVertexFocus];
                sourceCenteringW = pw[appSettings.previousVertexFocus];
                sourceCenteringU = pu[appSettings.previousVertexFocus];
            }

            if (vertexDistance[appSettings.currentVertexFocus] > appSettings.maximumConsideringDepth && vertexDistance[appSettings.previousVertexFocus] <= appSettings.maximumConsideringDepth) {
                appSettings.currentVertexFocus = appSettings.previousVertexFocus;
            }

            if (appSettings.currentVertexFocus >= 0 && vertexDistance[appSettings.currentVertexFocus] <= appSettings.maximumConsideringDepth) {
                targetCenteringX = px[appSettings.currentVertexFocus];
                targetCenteringY = py[appSettings.currentVertexFocus];
                targetCenteringZ = pz[appSettings.currentVertexFocus];
                targetCenteringW = pw[appSettings.currentVertexFocus];
                targetCenteringU = pu[appSettings.currentVertexFocus];
            }

            let targetFraction = appSettings.transitionFocusFraction;
            let sourceFraction = 1 - targetFraction;

            let centeringX = targetCenteringX;
            let centeringY = targetCenteringY;
            let centeringZ = targetCenteringZ;
            let centeringW = targetCenteringW;
            let centeringU = targetCenteringU;

            if (targetFraction == 1.0) {
                appSettings.previousVertexFocus = appSettings.currentVertexFocus;
            } else {
                centeringX = sourceFraction * sourceCenteringX + targetFraction * targetCenteringX;
                centeringY = sourceFraction * sourceCenteringY + targetFraction * targetCenteringY;
                centeringZ = sourceFraction * sourceCenteringZ + targetFraction * targetCenteringZ;
                centeringW = sourceFraction * sourceCenteringW + targetFraction * targetCenteringW;
                centeringU = sourceFraction * sourceCenteringU + targetFraction * targetCenteringU;
            }

            for (let i = 0; i < power; i++) {
                if (!isDiscovered[i]) {
                    continue;
                }
                else if (vertexDistance[i] > appSettings.maximumConsideringDepth) {
                    sphereGroup[i].visible = false;
                    continue;
                } else {
                    sphereGroup[i].visible = true;
                }
                px[i] -= centeringX;
                py[i] -= centeringY;
                pz[i] -= centeringZ;
                pw[i] -= centeringW;
                pu[i] -= centeringU;
                sphereGroup[i].position.set(
                    px[i],
                    appSettings.flatteningForceFraction == 1.0 ? 0.0 : py[i],
                    pz[i]
                );
                sphereGroup[i].lookAt(appSettings.camera.position);
            }
            for (let i = 0; i < power; i++) {
                if (!isDiscovered[i])
                    continue;
                let sphereIposition = sphereGroup[i].position;
                for (let index = 0; index < 2; index++) {
                    let connectionArrayIndex = index == 0 ? connectionA[i] : connectionB[i];

                    if (vertexDistance[i] > appSettings.maximumConsideringDepth
                        || vertexDistance[connectionArrayIndex] > appSettings.maximumConsideringDepth) {
                        if (arrows[2 * i + index] != undefined) {
                            arrows[2 * i + index].visible = false;
                            if (arrowTransitions[2 * i + index] != undefined)
                                arrowTransitions[2 * i + index].visible = false;
                        }

                        continue;
                    } else {
                        if (arrows[2 * i + index] != undefined) {
                            arrows[2 * i + index].visible = true;

                            if (arrowTransitions[2 * i + index] != undefined)
                                arrowTransitions[2 * i + index].visible = true;
                        }

                    }

                    if (connectionArrayIndex != i) {
                        let dist = sphereGroup[connectionArrayIndex].position.clone().sub(sphereIposition);
                        let targetMass = mass[connectionArrayIndex];
                        let len = dist.length() - targetMass;
                        arrows[2 * i + index].position.copy(sphereIposition);
                        arrows[2 * i + index].position.setY(arrows[2 * i + index].position.y + targetMass * ((i < connectionArrayIndex) ? arrowOffset : -arrowOffset));
                        dist.normalize();
                        arrows[2 * i + index].setDirection(dist);
                        arrows[2 * i + index].setLength(
                            len - targetMass * 0.1,
                            targetMass / 2,
                            targetMass / 2
                        );
                        if (arrowTransitions[2 * i + index] != undefined) {
                            arrowTransitions[2 * i + index].position.set(
                                sphereIposition.x + dist.x * len * 0.8,
                                sphereIposition.y + dist.y * len * 0.8,
                                sphereIposition.z + dist.z * len * 0.8
                            );
                            arrowTransitions[2 * i + index].lookAt(appSettings.camera.position);
                        }
                    }
                }
            }
        } else {
            for (let i = 0; i < power; i++) {
                if (!isDiscovered[i]) {
                    continue;
                }
                sphereGroup[i].lookAt(appSettings.camera.position);
            }
        }
    };
    return new Animatable(update, init, destroy);
}

class graphFactory {
    getRandomAutomaton(size, appSettings) {
        let goA = new Array(size);
        let goB = new Array(size);

        for (let i = 0; i < size; i++) {
            goA[i] = Math.floor(Math.random() * size);
            goB[i] = Math.floor(Math.random() * size);
        }

        return getAnimatableGraph([goA, goB], appSettings, "Some random automaton of size " + size);
    }

    getCernyAutomaton(size, appSettings) {
        let goA = new Array(size);
        let goB = new Array(size);

        for (let i = 0; i < size; i++) {
            goA[i] = i;
            goB[i] = i + 1;
        }
        goA[0] = 1;
        goB[size - 1] = 0;

        return getAnimatableGraph([goA, goB], appSettings, "Černý automaton of size " + size);
    }

    getKarisAutomaton(appSettings) {
        return getAnimatableGraph([
            [1, 2, 0, 5, 3, 4],
            [0, 1, 3, 2, 2, 5]
        ], appSettings, "Kari's automaton");
    }

    getExtreme4Automaton(appSettings) {
        return getAnimatableGraph([
            [0, 2, 1, 2],
            [3, 0, 2, 1]
        ], appSettings, "Extreme automaton of size 4");
    }

    getExtreme3Automaton(appSettings) {
        return getAnimatableGraph([
            [1, 0, 1],
            [1, 2, 0]
        ], appSettings, "Extreme automaton of size 3");
    }
}