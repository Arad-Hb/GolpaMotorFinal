const toggle=document.getElementById('themeToggle');

toggle.addEventListener('click',()=>{

document.body.classList.toggle('dark-mode');

localStorage.setItem(
'admin-theme',
document.body.classList.contains('dark-mode')
?'dark':'light'
);

});

if(localStorage.getItem('admin-theme')==='dark'){
document.body.classList.add('dark-mode');
}