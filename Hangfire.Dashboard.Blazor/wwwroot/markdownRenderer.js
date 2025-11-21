// markdownRenderer.js
import { marked } from '/_content/Hangfire.Dashboard.Blazor/lib/marked/marked.esm.js';

export function renderMarkdown(element, fileUrl) {
    return new Promise(async (resolve, reject) => {
        try {
            if (!fileUrl) {
                element.innerHTML = '<p style="color: red;">File URL not provided</p>';
                resolve();
                return;
            }

            const response = await fetch(fileUrl);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const content = await response.text();

            if (marked && content) {
                element.innerHTML = marked.parse(content);
            } else {
                element.innerHTML = '<p style="color: red;">Failed to parse markdown</p>';
            }

            resolve();
        } catch (error) {
            console.error('Error:', error);
            const element = document.getElementById(`md-${element}`);
            if (element) {
                element.innerHTML = '<p style="color: red;">Error rendering file: ' + error.message + '</p>';
            }
            reject(error);
        }
    });
}