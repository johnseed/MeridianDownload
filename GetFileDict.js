var trs = $('#mt130 > tbody')[0].children;
var fileDict = {};
for (let i = 1; i < trs.length; i++) {
    const tr = trs[i];
    let name = tr.children[2].innerText;
    let href = tr.children[4].children[0].href;
    fileDict[name] = href;
}
document.write(JSON.stringify(fileDict));