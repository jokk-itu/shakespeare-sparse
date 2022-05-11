var file = new File(Array.from(document.getElementsByTagName('blockquote')).map(x => x.innerText), "hamlet.txt");
var a = document.createElement('a'); 
a.onclick = async (event) => console.log(await file.text());
a.click();
