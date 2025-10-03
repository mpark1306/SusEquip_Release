function downloadFile(fileName, mimeType, base64Data) {
    var link = document.createElement('a');
    link.download = fileName;
    link.href = 'data:' + mimeType + ';base64,' + base64Data;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
