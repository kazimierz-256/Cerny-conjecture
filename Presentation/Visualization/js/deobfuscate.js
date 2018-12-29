let source = "js/pass_details.js";
let data = undefined;

$("#controls").click(() => {
    let w = new Worker(source);

    w.onmessage = (event) => {
        data = event.data;
    }
});

$("#creator-contact").click(() => {
    if (data != undefined) {
        let message = JSON.parse(CryptoJS.AES.decrypt(data, source).toString(CryptoJS.enc.Utf8))[0];
        $("#creator-contact").attr("href", message);
    } else {
        M.toast({ html: 'Please try again, something went wrong' });
    }
});

$("#creator-video").click(() => {
    if (data != undefined) {
        let message = JSON.parse(CryptoJS.AES.decrypt(data, source).toString(CryptoJS.enc.Utf8))[1];
        $("#creator-video").attr("href", message);
    } else {
        M.toast({ html: 'Please try again, something went wrong' });
    }
});