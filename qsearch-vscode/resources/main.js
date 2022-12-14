// This script will be run within the webview itself
// It cannot access the main VS Code APIs directly.

(function () {
    const vscode = acquireVsCodeApi();

    ////////////////////////////////////////////////////////////////////////////
    // Global UI elements initialization.
    ////////////////////////////////////////////////////////////////////////////
    const content = /** @type {HTMLDivElement} */ (document.getElementById('content'));
    let searchString = /** @type {HTMLInputElement} */ (document.getElementById('search-string'));
    let searchFile = /** @type {HTMLInputElement} */ (document.getElementById('search-file'));
    let caseSensitive = /** @type {HTMLInputElement} */ (document.getElementById('case-sensitive'));
    let allMatch = /** @type {HTMLInputElement} */ (document.getElementById('all-match'));
    let regexMatch = /** @type {HTMLInputElement} */ (document.getElementById('regex-match'));
    let searchResults = /** @type {HTMLTableElement} */ (document.getElementById('search-results'));
    let filePreview = /** @type {HTMLElement} */ (document.getElementById('file-preview'));

    function initSearchStringInput() {
        searchString = document.createElement('input');
        searchString.id = 'search-string';
        searchString.placeholder = 'String to search';
        content.appendChild(searchString);
        content.appendChild(document.createElement('br'));
    }
    function initSearchFileInput() {
        searchFile = document.createElement('input');
        searchFile.id = 'search-file';
        searchFile.placeholder = 'File to search'
        content.appendChild(searchFile);
        content.appendChild(document.createElement('br'));
    }
    function initSearchParamsCheckbox() {
        function createCheckboxAndLabel(/** @type {string} */ id, /** @type {string} */ labelText) {
            let checkbox = document.createElement('input');
            checkbox.id = id;
            checkbox.type = 'checkbox';
            checkbox.className = 'search-param-checkbox';
            let label = document.createElement('label');
            label.className = 'case-sensitive-text';
            label.appendChild(document.createTextNode(labelText));
            content.appendChild(checkbox);
            content.appendChild(label);
            return checkbox;
        }
        caseSensitive = createCheckboxAndLabel('case-sensitive', 'case sensitive');
        allMatch = createCheckboxAndLabel('all-match', 'all match');
        regexMatch = createCheckboxAndLabel('regex-match', 'regex match');
    }
    function initSearchButton() {
        let btn = document.createElement('button');
        btn.className = 'searchbtn'
        btn.innerText = 'Search';
        btn.onclick = function() {
            for (let i = searchResults.children.length - 1; i >= 0; i--) {
                searchResults.removeChild(searchResults.childNodes[i]);
            }
            filePreview.innerText = 'file-preview';
            let searchParams = {
                searchString: searchString.value,
                searchFile: searchFile.value,
                caseSensitive: caseSensitive.checked,
                allMatch: allMatch.checked,
                regexMatch: regexMatch.checked
            }
            sendMessageToExtension('StartSearch', searchParams);
        };
        content.appendChild(btn);
        content.appendChild(document.createElement('br'));
    }
    function initSearchResults() {
        let div = document.createElement('div');
        div.className = 'div-search-result';
        content.appendChild(div);
        searchResults = document.createElement('table');
        searchResults.className = 'search-results';
        div.appendChild(searchResults);
        let tr = document.createElement('tr');
        searchResults.appendChild(tr);
        let td1 = document.createElement('th');
        td1.innerText = 'file_name';
        tr.appendChild(td1);
        let td2 = document.createElement('th');
        td2.innerText = 'file_path';
        tr.appendChild(td2);
        let td3 = document.createElement('th');
        td3.innerText = 'count';
        tr.appendChild(td3);
    }
    function initFilePreview() {
        content.appendChild(document.createElement('br'));
        content.appendChild(document.createElement('br'));
        let div = document.createElement('div');
        div.className = 'div-file-preview';
        content.append(div);
        filePreview = document.createElement('label');
        filePreview.id = 'file-preview';
        filePreview.innerText = 'file-preview';
        filePreview.className = 'p-file-preview';
        div.appendChild(filePreview);
    }
    function updateResultsInfo(/** @type {Array<Object<string, string>>} */ results) {
        if (results.length == 0) {
            filePreview.innerText = 'no result find';
            return;
        }
        let tr = document.createElement('tr');
        searchResults.appendChild(tr);
        let td_title1 = document.createElement('th');
        td_title1.innerText = 'file_name';
        tr.appendChild(td_title1);
        let td_title2 = document.createElement('th');
        td_title2.innerText = 'file_path';
        tr.appendChild(td_title2);
        let td_title3 = document.createElement('th');
        td_title3.innerText = 'count';
        tr.appendChild(td_title3);
        results.forEach((value, _) => {
            let tr = document.createElement('tr');
            searchResults.appendChild(tr);
            let td1 = document.createElement('th');
            td1.innerText = /** @type {string} */ (value.fileName);
            tr.appendChild(td1);
            let td2 = document.createElement('th');
            td2.innerText = /** @type {string} */ (value.filePath);
            tr.appendChild(td2);
            let td3 = document.createElement('th');
            td3.innerText = /** @type {string} */ (value.count);
            tr.appendChild(td3);
            tr.onclick = function(event) {
                filePreview.innerText = /** @type {string} */ (value.fileData);
            }
            tr.ondblclick = function(event) {
                sendMessageToExtension('OpenFileInVSCode', {
                    filePath: value.filePath, 
                    startPos: value.startPos,
                    endPos: value.endPos});
            }
        });
    }
    
    // Add custom UI elements.
    {
        initSearchStringInput();
        initSearchFileInput();
        initSearchParamsCheckbox();
        initSearchButton();
        initSearchResults();
        initFilePreview();
    }

    ////////////////////////////////////////////////////////////////////////////
    // Message handlers, including send messages to ts and handle messages
    // from the extension.
    ////////////////////////////////////////////////////////////////////////////
    function sendMessageToExtension(/** @type {string} */ command, /** @type {any} */ text) {
        vscode.postMessage({command: command, text: text});
    }
    window.addEventListener('message', event => {
        const message = event.data; // The json data that the extension sent
        switch (message.command) {
            case 'ShowResult':
                updateResultsInfo(message.text);
                break;
        }
    });
    
    ////////////////////////////////////////////////////////////////////////////
    // Helper func.
    ////////////////////////////////////////////////////////////////////////////
}());

