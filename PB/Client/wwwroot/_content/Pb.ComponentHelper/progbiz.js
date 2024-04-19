
//Set color to span element
function setColor() {

}


//excel file download function

window.JSInteropExt = {};

window.JSInteropExt.saveAsFile = (filename, type, bytesBase64) => {

    if (navigator.msSaveBlob) {

        var data = window.atob(bytesBase64);

        var bytes = new Uint8Array(data.length);

        for (var i = 0; i < data.length; i++) {

            bytes[i] = data.charCodeAt(i);

        }

        var blob = new Blob([bytes.buffer], { type: type });

        navigator.msSaveBlob(blob, filename);

    }

    else {

        var link = document.createElement('a');

        link.download = filename;

        link.href = "data:" + type + ";base64," + bytesBase64;

        document.body.appendChild(link);

        link.click();

        document.body.removeChild(link);

    }

}

//print object throgh console
function logObjectToConsole(obj) {
    console.log(obj);
}

//open file selector
function openFileSelector(elementID = "", index = -1, needSetItemIndex = "0") {
    if (elementID != "") {
        const fileInput = document.getElementById(elementID);
        fileInput.click();

        if (needSetItemIndex == "1") {
            var componentRef = localStorage.setItem("imageItemIndex", index);
        }
    }
}

//Setting input element file
function setFileNameToInput(id, fileName) {
    const inputElement = document.getElementById(id);

    if (inputElement) {
        // Create a new File object with the desired file name.
        const fakeFile = new File([], fileName, { type: "application/octet-stream" });

        // Create a DataTransfer object and add the fake file to it.
        const dataTransfer = new DataTransfer();
        dataTransfer.items.add(fakeFile);

        // Set the files property of the input element to the DataTransfer object.
        inputElement.files = dataTransfer.files;
    }
}

//settig select element option as selected
function setSelectedOption(selectId, optionValue="0") {
    var selectElement = document.getElementById(selectId);

    if (!selectElement) {
        console.error("Select element not found with ID: " + selectId);
        return;
    }

    for (var i = 0; i < selectElement.options.length; i++) {
        var option = selectElement.options[i];
        if (option.value === optionValue) {
            option.selected = true;
        } else {
            option.selected = false;
        }
    }
}

//region Modal

function ShowModal(id, focusElement = "", errorDivID = "") {

    $("#" + id).modal("show");

    $('#' + id).on('shown.bs.modal', function () {
        if (focusElement != "") {
            $("#" + focusElement).focus();
        }

        if (errorDivID != "") {
            var div = document.getElementById(errorDivID);
            if (!div.classList.contains("d-none")) {
                div.classList.add("d-none");
            }
        }
    })
}


function HideModal(id) {
    $("#" + id).modal("hide");
}
function HideFilter(id) {
    $("#" + id).removeClass('show');
    $(".offcanvas-backdrop").removeClass('show');
}

function HideCanvas(id) {
    $("#" + id).offcanvas("hide");
}

//#region Message

function CustomConfirm(title, message, type, confirmButtonText = 'Confirm', cancelButtonText = 'Cancel') {
    return new Promise((resolve) => {
        Swal.fire({
            title: title,
            text: message,
            type: type,
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: confirmButtonText,
            cancelButtonText: cancelButtonText
        }).then((result) => {
            if (result.value) {
                resolve(true);
            } else {
                resolve(false);
            }
        });
    });
}

ErrorMessage = function (msg = "Oops..Something went wrong!", title = "Oops!!", ErrorCode = 0) {

    var div = document.getElementById("divError-" + ErrorCode);
    if (div != null) {
        div.innerText = msg;
        div.classList.remove("d-none");
        div.focus();
        setTimeout(function () {
            div.innerText = "";
            div.classList.add("d-none");
        }, 7000);
    }
    else if (msg != "Oops..Something went wrong!") {
        var msgLines = msg.split('\n');
        Swal.fire({ icon: 'error', title: title, html: msgLines.join('<br>'), });
    }
    else {
        Swal.fire({ icon: 'error', title: title, html: msg, });
    }
}

SuccessMessage = function (msg = "You're done!!", title = "", buttonText = "Great!", SuccessCode = 0) {

    var div = document.getElementById("divSuccess-" + SuccessCode);
    if (div != null) {
        div.innerText = msg;
        div.classList.remove("d-none");
        div.focus();

        setTimeout(function () {
            div.innerText = "";
            div.classList.add("d-none");
        }, 7000);
    }
    else {
        Swal.fire({
            title: `<strong>${title}</strong>`,
            icon: 'success',
            html: msg,
            showCloseButton: false,
            showCancelButton: false,
            focusConfirm: false,
            confirmButtonText:
                '<i class="fa fa-thumbs-up"></i> ' + buttonText,
            confirmButtonAriaLabel: 'Thumbs up, great!',
        })
    }
}

ShowMessage = function (msg, title, buttonText = "Great!") {
    Swal.fire({
        title: `<strong>${title}</strong>`,
        html: msg,
        showCloseButton: false,
        showCancelButton: false,
        focusConfirm: false,
        confirmButtonText:
            '<i class="fa fa-thumbs-up"></i> ' + buttonText,
        confirmButtonAriaLabel: 'Thumbs up, great!',
    });
}

//#endregion

//#region Focus

AutoFocus = function (id) {
    var element = document.getElementById(id);
    if (element != null)
        element.focus();
};

//#endregion

//#region Report Export

csvExport = function (gridId, reportName = "report") {
    $('#' + gridId).tableExport({ type: 'csv', fileName: reportName });
}

excelExport = function (gridId, reportName = "report") {
    //$('#' + gridId).tableExport({ type: 'excel', fileName: reportName });
    $('#' + gridId).tableExport({ type: 'excel' });
}

pdfExport = function (gridId, reportName = "report") {
    $("#" + gridId).tableExport({
        type: 'pdf',
        fileName: reportName,
        jspdf: {
            orientation: 'l',
            format: 'a3',
            margins: { left: 10, right: 10, top: 20, bottom: 20 },
            autotable: {
                styles: {
                    fillColor: 'inherit',
                    textColor: 'inherit'
                },
                tableWidth: 'auto'
            }
        }
    });
}

//#endregion

//#region API Call

GetAPI = function (apiName, successFun, reference) {

    var token = localStorage.getItem("accessToken");

    $.ajax({
        type: "GET",
        url: apiName,
        contentType: "application/json; charset=utf-8",
        beforeSend: function (xhr) {
            xhr.setRequestHeader('Authorization', `Bearer ${token}`);
        },
        success: function (data) {
            window[successFun](data, reference);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            //$("#msg").html(XMLHttpRequest.responseJSON.Message);
            //$("#div_message").removeClass().addClass('div_message alert alert-danger').show().delay(10000).hide("slow");
        }
    });
};

//#endregion

////#region Loading Icon

//function showLoading(divId = "", loaderType = 0) {
//    var loaderDiv = null;
//    var existingDiv = null;

//    if (loaderType === 0) {
//        loaderDiv = document.getElementById('side-app-loader');
//        if (loaderDiv != null) {
//            loaderDiv.classList.remove('d-none');
//        }
//    }
//    else {

//        switch (loaderType) {

//            case 1:

//                loaderDiv = document.getElementById("loader-div-1");

//                break;

//            case 2:

//                loaderDiv = document.getElementById("loader-div-2");

//                break;

//            case 3:

//                loaderDiv = document.getElementById("loader-div-3");

//                break;

//            case 4:

//                loaderDiv = document.getElementById("loader-div-4");

//                break;
//        }

//        existingDiv = document.getElementById(divId);

//        if (loaderDiv != null && existingDiv != null) {

//            if (loaderDiv.classList.contains('d-none'))
//                loaderDiv.classList.remove('d-none');

//            if (!existingDiv.classList.contains('d-none'))
//                loaderDiv.classList.add('d-none');
//        }
//    }

//}

//function hideLoading(divId = "", loaderType = 0) {

//    var loaderDiv = null;
//    var existingDiv = null;

//    if (loaderType === 0) {
//        loaderDiv = document.getElementById('side-app-loader');
//        if (loaderDiv != null) {
//            loaderDiv.classList.add('d-none');
//        }
//    }
//    else {

//        switch (loaderType) {
//            case 1:

//                loaderDiv = document.getElementById("loader-div-1");

//                break;

//            case 2:

//                loaderDiv = document.getElementById("loader-div-2");

//                break;

//            case 3:

//                loaderDiv = document.getElementById("loader-div-3");

//                break;

//            case 4:

//                loaderDiv = document.getElementById("loader-div-4");

//                break;
//        }

//        existingDiv = document.getElementById(divId);

//        if (loaderDiv != null && existingDiv != null) {

//            if (!loaderDiv.classList.contains('d-none'))
//                loaderDiv.classList.add('d-none');

//            if (existingDiv.classList.contains('d-none'))
//                loaderDiv.classList.remove('d-none');
//        }
//    }
//}



function showLoading1(divId = "", loaderType = 0) {
    var loaderDiv = null;
    var existingDiv = null;

    if (loaderType === 0) {
        loaderDiv = $('#side-app-loader');
        if (loaderDiv.length) {
            loaderDiv.removeClass('d-none');
        }
    } else {
        switch (loaderType) {
            case 1:
                loaderDiv = $('#loader-div-1');
                break;
            case 2:
                loaderDiv = $('#loader-div-2');
                break;
            case 3:
                loaderDiv = $('#loader-div-3');
                break;
            case 4:
                loaderDiv = $('#loader-div-4');
                break;
        }

        existingDiv = $('#' + divId);

        if (loaderDiv.length && existingDiv.length) {
            if (loaderDiv.hasClass('d-none')) {
                loaderDiv.removeClass('d-none');
            }
            if (!existingDiv.hasClass('d-none')) {
                loaderDiv.addClass('d-none');
            }
        }
    }
}

function hideLoading1(divId = "", loaderType = 0) {
    var loaderDiv = null;
    var existingDiv = null;

    if (loaderType === 0) {
        loaderDiv = $('#side-app-loader');
        if (loaderDiv.length) {
            loaderDiv.addClass('d-none');
        }
    } else {
        switch (loaderType) {
            case 1:
                loaderDiv = $('#loader-div-1');
                break;
            case 2:
                loaderDiv = $('#loader-div-2');
                break;
            case 3:
                loaderDiv = $('#loader-div-3');
                break;
            case 4:
                loaderDiv = $('#loader-div-4');
                break;
        }

        existingDiv = $('#' + divId);

        if (loaderDiv.length && existingDiv.length) {
            if (!loaderDiv.hasClass('d-none')) {
                loaderDiv.addClass('d-none');
            }
            if (existingDiv.hasClass('d-none')) {
                loaderDiv.removeClass('d-none');
            }
        }
    }
}

function showLoading(divId = "") {
    if (divId != "")
        $("#" + divId).addClass("placeholder-glow");
    else
        $('#please-wait').css('display', 'flex');
}

function hideLoading(divId = "") {
    if (divId != "")
        $("#" + divId).removeClass("placeholder-glow");
    else
        $('#please-wait').css('display', 'none');
}

//#endregion

//#region Basic Functions

function topFunction() {
    document.body.scrollTop = 0; // For Safari
    document.documentElement.scrollTop = 0; // For Chrome, Firefox, IE and Opera
}


HideModel = function (id) {
    $('#' + id).modal('hide');
}

//#endregion

//#region Print Quotation PDF

PrintPdfDocument = function (htmlElement = "") {

    if (htmlElement != "") {

        var newWin = window.open('', 'Quotation');

        newWin.document.open();

        newWin.document.write(htmlElement);


    }
}


printDiv = function (id) {
    var divToPrint = document.getElementById(id);

    var newWin = window.open('', 'Print-Window');

    newWin.document.open();

    newWin.document.write('<html>' +
        '<body onload = "window.print()" > ' +
        '' +
        '<script src="/assets/js/jquery.3.2.1.min.js"></script> ' +
        '<script src = "/assets/js/bootstrap.min.js"></script>' +
        '<link href="/assets/css/bootstrap.min.css" rel="stylesheet" />' +
        '<link href = "/default/css/pos_bill.css" rel = "stylesheet" />' +
        divToPrint.innerHTML +
        '</body></html>');

    newWin.document.close();

    setTimeout(function () { newWin.close(); }, 10);
}

//#endregion

//#region Service Worker

//navigator.serviceWorker.register('service-worker.js');

//if ('serviceWorker' in navigator) {
//    navigator.serviceWorker
//        .register('/sw.js', { scope: '/' })
//        .then(() => {
//            console.info('Developer for Life Service Worker Registered');
//        }, err => console.error("Developer for Life Service Worker registration failed: ", err));

//    navigator.serviceWorker
//        .ready
//        .then(() => {
//            console.info('Developer for Life Service Worker Ready');
//        });
//}

//#endregion

//#region Number Textbox

$(document).on("keypress", ".decimal", function (event) {
    return isDecimal(event, this);
});

$(document).on("keypress", ".integer", function (event) {
    return isInteger(event, this);
});

$(document).on("keypress", ".negativenumber", function (event) {
    return isNegativeNumber(event, this);
});

function isDecimal(evt, element) {
    var charcode = evt.which ? evt.which : event.KeyCode;
    if (charcode == 8 || (charcode == 46 && $(element).val().indexOf('.') == -1) || event.keyCode == 37 || event.keyCode == 39) {
        return true;
    }
    else if (charcode < 48 || charcode > 57) {
        return false;
    }
    else return true;
}

function isInteger(evt, element) {
    var charcode = evt.which ? evt.which : event.KeyCode;
    if (charcode == 8 || event.keyCode == 37 || event.keyCode == 39) {
        return true;
    }
    else if (charcode < 48 || charcode > 57) {
        return false;
    }
    else return true;
}

function isNegativeNumber(evt, element) {
    var charcode = evt.which ? evt.which : event.KeyCode;
    if (charcode == 8 || (charcode == 45 && $(element).val().indexOf('-') == -1) || (charcode == 46 && $(element).val().indexOf('.') == -1) || event.keyCode == 37 || event.keyCode == 39) {
        return true;
    }
    else if (charcode < 48 || charcode > 57) {
        return false;
    }
    else return true;
}

//#endregion


//#region Culture

window.blazorCulture = {
    get: () => window.localStorage['BlazorCulture'],
    set: (value) => window.localStorage['BlazorCulture'] = value
};

window.blazorDirection = {
    get: () => window.localStorage['BlazorDirection'],
    set: (value) => window.localStorage['BlazorDirection'] = value
};

function setProjectDirection() {
    var dir = localStorage.getItem("BlazorDirection");
    var lang = localStorage.getItem("BlazorCulture");
    $('html').attr('dir', dir);
    $('html').attr('lang', lang);
    if (dir == "rtl") {
        $('#lang-img').attr('src', 'assets/images/flags/arab.jpg');
        $('#dir').attr('href', 'assets/css/style-rtl.css');
        $('#lang-name').text('عربي');
        localStorage.setItem("rtl", true);
        localStorage.removeItem("ltr");
        $('body').addClass("rtl");
        $('body').removeClass("ltr");
    }
    else {
        $('#lang-img').attr('src', 'assets/images/flags/en.png');
        $('#dir').attr('href', 'assets/css/style.css');
        $('#lang-name').text('English');
        localStorage.setItem("ltr", true);
        localStorage.removeItem("rtl");
        $('body').addClass("ltr");
        $('body').removeClass("rtl");
    }
}

//#endregion





function ShowModal(id = "popup") {
    $("#" + id).modal("show");
}


function HideModal(id = "popup") {
    $("#" + id).modal("hide");
}




//Function for focusig an element
function focusElement(elementId) {
    // Get the element using its ID
    var element = document.getElementById(elementId);

    // Set focus on the element
    if (element) {
        element.focus();
    }
}

//Reading an element value
function getElementValue(id = "") {
    if (id != "") {
        var inputElement = document.getElementById(id);
        var inputValue = inputElement.value;
        return inputValue;
    }
}

//Setting an element value
function setElementValue(id = "", value = "") {
    if (id != "" && value != "") {
        var inputElement = document.getElementById(id);
        inputElement.value = value;
    }
}

//Dropdown select relaed functions

function dropdownClicked(dropdownID = "", listID = "") {
    if (dropdownID != "" && listID != "") {
        const myDiv = document.querySelector('#' + dropdownID);
        const myToggle = document.querySelector('#' + listID);

        myDiv.addEventListener('click', function (event) {
            myToggle.classList.toggle('show');

            const ulElement = event.target.querySelector('ul');
            const liElements = ulElement.querySelectorAll('li');
            const activeLiElement = ulElement.querySelector('.active');

            if (!activeLiElement) {
                liElements[0].classList.add('active');
                liElements[0].focus();
            }
        });

    }
}


function setDropdownFocus(dropdownID = "", className = "") {
    if (dropdownID != "" && className != "") {
        const dropDown = document.getElementById(dropdownID);

        if (dropDown != null && !dropDown.classList.contains(className)) {
            dropDown.classList.add(className);
        }

        document.addEventListener('click', (event) => {
            // If the clicked element is not inside the div, do something
            if (!dropDown.contains(event.target)) {
                removeDropdownFocus(dropdownID, className);
            }
        });

        dropDown.addEventListener('keydown', (event) => {
            // Use the bootstrap dropdown method to show the dropdown
            if (event.code === 'Enter' || event.code === 'NumpadEnter') {
                const dropdown = new bootstrap.Dropdown(dropDown);
                dropdown.show();
                var textBox = dropDown.querySelector("input[type='text']");
                if (textBox) {
                    textBox.focus();
                }
            }
        });
    }
}

function removeDropdownFocus(dropdownID = "", className = "") {
    if (dropdownID != "" && className != "") {
        var dropDown = document.getElementById(dropdownID);
        if (dropDown != null && dropDown.classList.contains(className)) {
            dropDown.classList.remove(className);
        }
    }
}

//List page Filter option functions

function initializeMenuSettings(menuID = "") {
    if (menuID !== "") {
        $('#' + menuID).on('show.bs.dropdown', function (event) {
            event.preventDefault();
        });
    }
}





function hideMenu(menuID = "") {
    if (menuID != "") {
        const myToggle = document.getElementById(menuID);
        if (myToggle.classList.contains('show')) {
            myToggle.classList.remove('show');
        }
    }
}





function checkAll() {
    var checkboxes = document.getElementById('myDiv').getElementsByTagName('input');
    for (var i = 0; i < checkboxes.length; i++) {
        if (checkboxes[i].type == 'checkbox') {
            checkboxes[i].checked = true;
        }
    }
}

function GetUCTMinute() {
    return (new Date().getTimezoneOffset()) * -1;
}

function ShowAttachment(id) {
    $('#' + id).toggleClass('show');
    document.addEventListener("click", function (event) {
        const targetElement = event.target;

        if (targetElement !== "divEmoji" && targetElement !== "divOpenEmoji") {
            $("#divEmoji").style.display = "none";
        }
    });
}

function HideAttachment(id) {
    $('#' + id).removeClass('show');
}

function BgImage() {
    var chatBody = document.getElementById('ChatBody');

    if (chatpresent) {
        chatBody.style.backgroundImage = 'none';
    } else {
        chatBody.style.backgroundImage = "url(''/assets/images/emptyscreen/dashboard.svg')";
        chatBody.style.backgroundRepeat = 'no-repeat';
        chatBody.style.backgroundPosition = 'center';
    }
}

function SetEmoji(pageObj) {
    document.querySelector('emoji-picker')
        .addEventListener('emoji-click', (event) => {
            pageObj.invokeMethod("SelectEmoji", `${event.detail.emoji.unicode}`);
        });
}


function ShowHiddenDiv(id) {
    let myDiv = document.getElementById(id);
    let toggleButton = document.getElementById("toggle-button");

    toggleButton.addEventListener("click", function () {
        if (myDiv.style.display === "none") {
            myDiv.style.display = "block";
        } else {
            myDiv.style.display = "none";
        }
    });
}

function SelectMenu(id) {
    let myDiv = document.getElementById(id);
    var menuElements = document.getElementsByClassName("menu");

    for (var i = 0; i < menuElements.length; i++) {
        var menuElement = menuElements[i];
        menuElement.classList.remove("active");
    }

    myDiv.classList.add("active");


}


function FixedModal(id) {
    $('#' + id).modal({
        backdrop: 'static',
        keyboard: false
    });
}

//$(document).ready(function () {
//    $('#modaldemo8').modal({
//        backdrop: 'static',
//        keyboard: false
//    })
//});


function downloadPDF(pdf, filename = "doc.pdf") {
    const linkSource = `data:application/pdf;base64,${pdf}`;
    const downloadLink = document.createElement("a");
    downloadLink.href = linkSource;
    downloadLink.download = filename;
    downloadLink.click();
}


function togglePasswordVisibility(id = "") {
    if (id != "") {
        var passwordField = document.getElementById(id);
    }
    var showPasswordIcon = document.getElementById("show-password-icon");

    if (passwordField.type === "password") {
        passwordField.type = "text";
        showPasswordIcon.classList.remove("fa-eye");
        showPasswordIcon.classList.add("fa-eye-slash");
    } else {
        passwordField.type = "password";
        showPasswordIcon.classList.remove("fa-eye-slash");
        showPasswordIcon.classList.add("fa-eye");
    }
}

function captureAndDownload() {
    const divToCapture = document.getElementById("divToCapture");

    // Use html2canvas to capture the div as an image
    html2canvas(divToCapture).then(function (canvas) {
        // Convert the canvas to a data URL (PNG by default)
        const dataUrl = canvas.toDataURL();

        // Create a temporary link element
        const a = document.createElement("a");
        a.href = dataUrl;
        a.download = "captured-image.png"; // Set the filename

        // Trigger a click event to initiate the download
        a.click();
    });
}

function buttonClick(id) {
    $("#" + id)[0].click();
}

