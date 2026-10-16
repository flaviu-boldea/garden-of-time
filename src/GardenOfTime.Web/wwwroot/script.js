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

(() => {
  const root = document.querySelector("[data-calendar]");
  if (!root) return;
  const API = "https://gradina-timpului-api-cvgdb2ffa0bvemf4.centralus-01.azurewebsites.net/api/availability/";
  const grid = root.querySelector(".cal-grid");
  const title = root.querySelector(".cal-title");
  const status = root.querySelector(".cal-status");
  const today = new Date(); today.setHours(0, 0, 0, 0);
  let view = new Date(today.getFullYear(), today.getMonth(), 1);
  let selected = null;
  let availabilityMonth = "";
  let unavailableDates = new Set();
  const iso = (d) => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;

  const describe = (d) => {
    if (typeof d === "boolean") return d ? "Zi disponibilă." : "Zi indisponibilă.";
    if (d && typeof d === "object") {
      const flag = d.available ?? d.isAvailable;
      const n = d.remaining ?? d.spotsLeft ?? d.availableSpots ?? d.slots;
      if (typeof flag === "boolean") return (flag ? "Zi disponibilă" : "Zi indisponibilă") + (typeof n === "number" ? ` (${n} locuri)` : ".");
      return Object.entries(d).map(([k, v]) => `${k}: ${typeof v === "object" ? JSON.stringify(v) : v}`).join(" · ");
    }
    return String(d);
  };

  const select = async (date) => {
    selected = iso(date);
    render();
    status.textContent = "Se încarcă…";
    try {
      const r = await fetch(API + selected);
      if (!r.ok) throw new Error(r.status);
      const ct = r.headers.get("content-type") || "";
      const d = ct.includes("json") ? await r.json() : await r.text();
      if (selected === iso(date)) status.textContent = `${date.toLocaleDateString("ro-RO", { day: "numeric", month: "long", year: "numeric" })}: ${describe(d)}`;
    } catch {
      status.textContent = "Nu am putut verifica disponibilitatea. Încearcă din nou.";
    }
  };

  const loadUnavailableDates = async () => {
    const year = view.getFullYear();
    const month = view.getMonth() + 1;
    const key = `${year}-${month}`;
    if (availabilityMonth === key) return;
    availabilityMonth = key;
    unavailableDates = new Set();
    try {
      const r = await fetch(`${API}${year}/${month}`);
      if (!r.ok) throw new Error(r.status);
      const d = await r.json();
      if (availabilityMonth !== key) return;
      unavailableDates = new Set(Array.isArray(d.unavailableDates) ? d.unavailableDates : []);
      render();
    } catch {
      if (availabilityMonth === key) status.textContent = "Nu am putut încărca zilele ocupate.";
    }
  };

  const render = () => {
    grid.textContent = "";
    title.textContent = view.toLocaleDateString("ro-RO", { month: "long", year: "numeric" });
    ["L", "M", "M", "J", "V", "S", "D"].forEach((n) => {
      const s = document.createElement("span"); s.className = "dow"; s.textContent = n; grid.append(s);
    });
    const offset = (view.getDay() + 6) % 7;
    for (let i = 0; i < offset; i++) grid.append(document.createElement("span"));
    const days = new Date(view.getFullYear(), view.getMonth() + 1, 0).getDate();
    for (let n = 1; n <= days; n++) {
      const date = new Date(view.getFullYear(), view.getMonth(), n);
      const b = document.createElement("button");
      const unavailable = unavailableDates.has(iso(date));
      b.type = "button"; b.textContent = n; b.disabled = date < today || unavailable;
      if (unavailable) b.classList.add("is-unavailable");
      b.setAttribute("aria-label", `${date.toLocaleDateString("ro-RO", { day: "numeric", month: "long" })}${unavailable ? ", zi ocupată" : ""}`);
      b.setAttribute("aria-pressed", String(iso(date) === selected));
      b.addEventListener("click", () => select(date));
      grid.append(b);
    }
    loadUnavailableDates();
  };

  root.querySelector(".cal-prev").addEventListener("click", () => { view = new Date(view.getFullYear(), view.getMonth() - 1, 1); render(); });
  root.querySelector(".cal-next").addEventListener("click", () => { view = new Date(view.getFullYear(), view.getMonth() + 1, 1); render(); });
  render();
})();
