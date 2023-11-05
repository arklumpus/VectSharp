
        /*
            VectSharp - A light library for C# vector graphics.
            Copyright (C) 2022 Giorgio Bianchini, University of Bristol

            Permission is hereby granted, free of charge, to any person obtaining
            a copy of this software and associated documentation files (the "Software"),
            to deal in the Software without restriction, including without limitation
            the rights to use, copy, modify, merge, publish, distribute, sublicense,
            and/or sell copies of the Software, and to permit persons to whom the
            Software is furnished to do so, subject to the following conditions:

            The above copyright notice and this permission notice shall be included in
            all copies or substantial portions of the Software.

            THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
            IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
            FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
            AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
            LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
            FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
            IN THE SOFTWARE.
        */

        let totalLength = @@totalLength@@;
        let isPlaying = true;

        document.getElementById("playButton").style.display = "none";
        document.getElementById("playButtonHitbox").style.display = "none";

        document.getElementById("playButtonHitbox").addEventListener("click", function () {
            let svgElements = document.getElementsByTagName("svg");

            let hasFinished = svgElements[0].getCurrentTime() >= totalLength;

            if (!hasFinished) {
                for (let i = 0; i < svgElements.length; i++) {
                    svgElements[i].unpauseAnimations();
                }

                isPlaying = true;

                document.getElementById("playButton").style.display = "none";
                document.getElementById("playButtonHitbox").style.display = "none";
                document.getElementById("pauseButton").style.display = "";
                document.getElementById("pauseButtonHitbox").style.display = "";
            }
            else {
                for (let i = 0; i < svgElements.length; i++) {
                    svgElements[i].setCurrentTime(0);
                    svgElements[i].unpauseAnimations();
                }

                isPlaying = true;

                document.getElementById("playButton").style.display = "none";
                document.getElementById("playButtonHitbox").style.display = "none";
                document.getElementById("pauseButton").style.display = "";
                document.getElementById("pauseButtonHitbox").style.display = "";
            }
        });

        document.getElementById("pauseButtonHitbox").addEventListener("click", function () {
            let svgElements = document.getElementsByTagName("svg");

            for (let i = 0; i < svgElements.length; i++) {
                svgElements[i].pauseAnimations();
            }

            isPlaying = false;

            document.getElementById("playButton").style.display = "";
            document.getElementById("playButtonHitbox").style.display = "";
            document.getElementById("pauseButton").style.display = "none";
            document.getElementById("pauseButtonHitbox").style.display = "none";
        });

        document.getElementById("timeLineBackground").addEventListener("mousedown", function (event) {
            let bounds = document.getElementById("timeLine").getBoundingClientRect();

            let position = (event.clientX - bounds.left) / bounds.width;

            if (position >= 0 && position <= 1) {
                let svgElements = document.getElementsByTagName("svg");

                for (let i = 0; i < svgElements.length; i++) {
                    svgElements[i].setCurrentTime(position * totalLength);
                }
            }
        });

        document.getElementById("timeLine").addEventListener("mousedown", function (event) {
            let bounds = document.getElementById("timeLine").getBoundingClientRect();

            let position = (event.clientX - bounds.left) / bounds.width;

            if (position >= 0 && position <= 1) {
                let svgElements = document.getElementsByTagName("svg");

                for (let i = 0; i < svgElements.length; i++) {
                    svgElements[i].setCurrentTime(position * totalLength);
                }
            }
        });

        document.getElementById("timeLineBackground").addEventListener("mousemove", function (event) {
            if (event.which == 1) {
                let bounds = document.getElementById("timeLine").getBoundingClientRect();

                let position = (event.clientX - bounds.left) / bounds.width;

                if (position >= 0 && position <= 1) {
                    let svgElements = document.getElementsByTagName("svg");

                    for (let i = 0; i < svgElements.length; i++) {
                        svgElements[i].setCurrentTime(position * totalLength);
                    }
                }
            }
        });

        document.getElementById("timeLine").addEventListener("mousemove", function (event) {
            if (event.which == 1) {
                let bounds = document.getElementById("timeLine").getBoundingClientRect();

                let position = (event.clientX - bounds.left) / bounds.width;

                if (position >= 0 && position <= 1) {
                    let svgElements = document.getElementsByTagName("svg");

                    for (let i = 0; i < svgElements.length; i++) {
                        svgElements[i].setCurrentTime(position * totalLength);
                    }
                }
            }
        });

        document.getElementById("timeLineStroke").style.pointerEvents = "none";

        document.getElementById("transition1://thumb").getElementsByTagName("animate")[0].addEventListener("endEvent", function () {
            document.getElementById("playButton").style.display = "";
            document.getElementById("playButtonHitbox").style.display = "";
            document.getElementById("pauseButton").style.display = "none";
            document.getElementById("pauseButtonHitbox").style.display = "none";


            let svgElements = document.getElementsByTagName("svg");
            for (let i = 0; i < svgElements.length; i++) {
                svgElements[i].pauseAnimations();
            }
        });

        let hideTimeout = setTimeout(function () { document.getElementById("animationControls").style.opacity = 0; }, 1500);

        document.addEventListener("mousemove", function () {
            clearTimeout(hideTimeout);
            document.getElementById("animationControls").style.opacity = 1;
            hideTimeout = setTimeout(function () { document.getElementById("animationControls").style.opacity = 0; }, 1500);
        });
    