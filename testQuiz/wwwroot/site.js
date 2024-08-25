function goBack() {
    window.history.back();
}

function openFullscreen() {
    var elem = document.documentElement;
    if (elem.requestFullscreen) {
        elem.requestFullscreen();
    } else if (elem.mozRequestFullScreen) { // Firefox
        elem.mozRequestFullScreen();
    } else if (elem.webkitRequestFullscreen) { // Chrome, Safari & Opera
        elem.webkitRequestFullscreen();
    } else if (elem.msRequestFullscreen) { // IE/Edge
        elem.msRequestFullscreen();
    }
}

function exitFullscreen() {
    if (document.exitFullscreen) {
        document.exitFullscreen();
    } else if (document.mozCancelFullScreen) { // Firefox
        document.mozCancelFullScreen();
    } else if (document.webkitExitFullscreen) { // Chrome, Safari & Opera
        document.webkitExitFullscreen();
    } else if (document.msExitFullscreen) { // IE/Edge
        document.msExitFullscreen();
    }
}

function addEventListenersForPage2() {
    document.addEventListener("fullscreenchange", triggerEvent);
    window.addEventListener("beforeunload", triggerEvent);
    window.addEventListener("resize", triggerEvent);
}

function triggerEvent() {
    DotNet.invokeMethodAsync('testQuiz', 'IncrementEventCount');
}

document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll('.questions-nav a').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault(); // Ngăn chặn hành vi mặc định của thẻ a
            const targetId = this.getAttribute('href').substring(1); // Lấy ID từ href
            const targetElement = document.getElementById(targetId);
            if (targetElement) {
                targetElement.scrollIntoView({ behavior: 'smooth' }); // Cuộn trang đến phần tử
            }
        });
    });
});