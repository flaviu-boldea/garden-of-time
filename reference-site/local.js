/* No Google runtime, analytics, cookies, build tools or API keys required. */
const header = document.querySelector('.local-header');
const menu = document.querySelector('.local-menu-button');
menu?.addEventListener('click', () => {
  const open = header.classList.toggle('local-open');
  menu.setAttribute('aria-expanded', String(open));
});
document.querySelectorAll('.local-submenu-button').forEach(button => {
  button.addEventListener('click', () => {
    const open = button.parentElement.classList.toggle('local-open');
    button.setAttribute('aria-expanded', String(open));
  });
});
document.addEventListener('keydown', event => {
  if (event.key !== 'Escape') return;
  header?.classList.remove('local-open');
  document.querySelectorAll('.local-open').forEach(element => element.classList.remove('local-open'));
  document.querySelectorAll('.local-header [aria-expanded]').forEach(element => element.setAttribute('aria-expanded', 'false'));
});
document.querySelector('[aria-label="Scroll down"]')?.addEventListener('click', () => {
  const sections = document.querySelectorAll('section');
  sections[1]?.scrollIntoView({ behavior: 'smooth' });
});