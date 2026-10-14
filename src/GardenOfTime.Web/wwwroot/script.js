const menuToggle = document.querySelector(".menu-toggle");
const mainNav = document.querySelector("#main-nav");

menuToggle?.addEventListener("click", () => {
  const isOpen = menuToggle.getAttribute("aria-expanded") === "true";
  menuToggle.setAttribute("aria-expanded", String(!isOpen));
  mainNav.classList.toggle("is-open", !isOpen);
});

mainNav?.querySelectorAll("a").forEach((link) => {
  link.addEventListener("click", () => {
    menuToggle?.setAttribute("aria-expanded", "false");
    mainNav.classList.remove("is-open");
  });
});

(() => {
  const history = [];
  const root = document.createElement("div");
  root.className = "chat-widget";
  root.innerHTML = `
    <button class="chat-launcher" type="button" aria-expanded="false" aria-controls="chat-panel">Chat</button>
    <section class="chat-panel" id="chat-panel" aria-label="Chat Grădina Timpului" hidden>
      <header class="chat-head"><strong>Grădina Timpului</strong><button class="chat-close" type="button" aria-label="Închide chatul">×</button></header>
      <div class="chat-log" role="log" aria-live="polite"></div>
      <form class="chat-form">
        <input type="text" name="message" autocomplete="off" required aria-label="Mesajul tău" placeholder="Scrie un mesaj...">
        <button type="submit">Trimite</button>
      </form>
    </section>`;
  document.body.append(root);

  const launcher = root.querySelector(".chat-launcher");
  const panel = root.querySelector(".chat-panel");
  const log = root.querySelector(".chat-log");
  const form = root.querySelector(".chat-form");
  const input = form.elements.message;

  const setOpen = (open) => {
    panel.hidden = !open;
    launcher.setAttribute("aria-expanded", String(open));
    if (open) input.focus();
  };
  const add = (who, text) => {
    const p = document.createElement("p");
    p.className = "chat-msg chat-" + who;
    p.textContent = text;
    log.append(p);
    log.scrollTop = log.scrollHeight;
  };

  launcher.addEventListener("click", () => setOpen(panel.hidden));
  root.querySelector(".chat-close").addEventListener("click", () => setOpen(false));
  document.querySelectorAll("[data-open-chat]").forEach((b) => b.addEventListener("click", () => setOpen(true)));
  add("bot", "Bună! Cu ce te putem ajuta?");

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const message = input.value.trim();
    if (!message) return;
    input.value = "";
    add("user", message);
    let reply;
    try {
      const r = await fetch("/api/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ message, history }),
      });
      const d = await r.json();
      reply = d.reply ?? d.error ?? "A apărut o eroare.";
    } catch {
      reply = "Nu am putut trimite mesajul. Încearcă din nou.";
    }
    history.push({ role: "user", content: message }, { role: "assistant", content: reply });
    add("bot", reply);
  });
})();
