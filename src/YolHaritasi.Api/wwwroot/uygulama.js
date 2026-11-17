// ============================================================================
//  ARAYÜZÜN JAVASCRIPT KODU
//  Her fonksiyon API'mize bir HTTP isteği gönderir ve gelen JSON'u ekrana çizer.
//  İstekleri tarayıcıda F12 → Network sekmesinde canlı olarak görebilirsin.
// ============================================================================

// --- API ile konuşan yardımcı fonksiyon ------------------------------------
// fetch: Tarayıcının HTTP istemcisi. "await" cevabı beklemek demek.
async function api(adres, secenekler = {}) {
  const cevap = await fetch(adres, {
    headers: { "Content-Type": "application/json" },
    ...secenekler,
  });
  if (!cevap.ok) {
    // 404, 500 gibi bir durum kodu geldi
    const hata = await cevap.json().catch(() => ({}));
    throw new Error(hata.mesaj ?? `Sunucu hatası: ${cevap.status}`);
  }
  // 204 No Content cevabının gövdesi yoktur
  return cevap.status === 204 ? null : cevap.json();
}

// Kullanıcıdan/veritabanından gelen metni HTML'e güvenle koymak için.
// (Aksi halde metin içindeki < > karakterleri HTML gibi çalışabilir: XSS saldırısı)
function guvenli(metin) {
  const d = document.createElement("div");
  d.textContent = metin ?? "";
  return d.innerHTML;
}

// --- Sekmeler ----------------------------------------------------------------
document.querySelectorAll(".sekme").forEach((dugme) => {
  dugme.addEventListener("click", () => {
    document.querySelectorAll(".sekme").forEach((d) => d.classList.remove("aktif"));
    document.querySelectorAll(".panel").forEach((p) => p.classList.remove("aktif"));
    dugme.classList.add("aktif");
    document.getElementById(dugme.dataset.sekme).classList.add("aktif");

    if (dugme.dataset.sekme === "alanlar") alanlariYukle();
    if (dugme.dataset.sekme === "sozluk") sozlukAra("");
  });
});

// --- Konu kartı (Bugün ekranında ve alan detayında ortak kullanılır) --------
function konuKartiHtml(konu) {
  const tamam = konu.tamamlanmaTarihi !== null;
  return `
    <div class="kart">
      <div class="meta">
        <span class="rozet">${guvenli(konu.alanAdi)}</span>
        <span class="rozet">⏱️ ~${konu.tahminiDakika} dk</span>
        ${tamam ? '<span class="rozet tamam">✓ Tamamlandı</span>' : ""}
      </div>
      <h2>${guvenli(konu.baslik)}</h2>
      <p>${guvenli(konu.ozet)}</p>

      <h3>📖 Terimler</h3>
      ${konu.terimler.map((t) => `
        <div class="terim">
          <div class="terim-ad">${guvenli(t.ad)}</div>
          <div>${guvenli(t.tanim)}</div>
          ${t.ornek ? `<div class="terim-ornek">${guvenli(t.ornek)}</div>` : ""}
        </div>`).join("")}

      <h3>🛠️ Görevler</h3>
      ${konu.gorevler.map((g) => `
        <label class="gorev ${g.tamamlandi ? "bitti" : ""}">
          <input type="checkbox" data-gorev="${g.id}" ${g.tamamlandi ? "checked" : ""}>
          <span>${guvenli(g.aciklama)}</span>
        </label>`).join("")}

      <button class="dugme" data-tamamla="${konu.id}" ${tamam ? "disabled" : ""}>
        ${tamam ? "✓ Bu konu tamamlandı" : "Konuyu tamamladım"}
      </button>
      <div class="api-etiket" style="margin:12px 0 0 8px">POST /api/konular/${konu.id}/tamamla</div>
    </div>`;
}

// Konu kartındaki görev kutuları ve "tamamladım" düğmesi için olay bağlama
function konuKartiOlaylari(kapsayici, yenile) {
  kapsayici.querySelectorAll("[data-gorev]").forEach((kutu) => {
    kutu.addEventListener("change", async () => {
      // PATCH /api/gorevler/{id}  gövde: { "tamamlandi": true/false }
      await api(`/api/gorevler/${kutu.dataset.gorev}`, {
        method: "PATCH",
        body: JSON.stringify({ tamamlandi: kutu.checked }),
      });
      kutu.closest(".gorev").classList.toggle("bitti", kutu.checked);
    });
  });

  kapsayici.querySelectorAll("[data-tamamla]").forEach((dugme) => {
    dugme.addEventListener("click", async () => {
      await api(`/api/konular/${dugme.dataset.tamamla}/tamamla`, { method: "POST" });
      await yenile();
    });
  });
}

// --- 📅 Bugün ---------------------------------------------------------------
async function bugunuYukle() {
  const alan = document.getElementById("bugunIcerik");
  try {
    const bugun = await api("/api/bugun");
    genelIlerlemeyiGoster(bugun.genelYuzdeIlerleme);

    if (!bugun.konu) {
      alan.innerHTML = `<div class="kart"><h2>${guvenli(bugun.mesaj)}</h2></div>`;
      return;
    }
    alan.innerHTML = `<p class="meta">Sıradaki konun bu. Terimleri oku, görevleri yap, bitince "Konuyu tamamladım"a bas.</p>`
      + konuKartiHtml(bugun.konu);
    konuKartiOlaylari(alan, bugunuYukle);
  } catch (e) {
    alan.innerHTML = `<div class="kart">⚠️ ${guvenli(e.message)}</div>`;
  }
}

function genelIlerlemeyiGoster(yuzde) {
  document.getElementById("genelYuzde").textContent = `%${yuzde}`;
  document.getElementById("genelCubuk").style.width = `${yuzde}%`;
}

// --- 🧭 Alanlar -------------------------------------------------------------
let seciliAlanId = null;

async function alanlariYukle() {
  const alanlar = await api("/api/alanlar");
  const liste = document.getElementById("alanListesi");
  liste.innerHTML = alanlar.map((a) => `
    <div class="kart alan-kart ${a.id === seciliAlanId ? "secili" : ""}" data-alan="${a.id}">
      <h4>${guvenli(a.ad)}</h4>
      <div class="meta">${guvenli(a.aciklama)}</div>
      <div class="cubuk"><div class="dolgu" style="width:${a.yuzdeIlerleme}%"></div></div>
      <div class="meta">${a.tamamlananKonuSayisi} / ${a.konuSayisi} konu · %${a.yuzdeIlerleme}</div>
    </div>`).join("");

  liste.querySelectorAll("[data-alan]").forEach((kart) =>
    kart.addEventListener("click", () => alanDetayiniGoster(Number(kart.dataset.alan))));

  if (seciliAlanId) await alanDetayiniGoster(seciliAlanId);
}

async function alanDetayiniGoster(id) {
  seciliAlanId = id;
  document.querySelectorAll(".alan-kart").forEach((k) =>
    k.classList.toggle("secili", Number(k.dataset.alan) === id));

  const alan = await api(`/api/alanlar/${id}`);
  const detay = document.getElementById("alanDetay");
  detay.innerHTML = `
    <div class="kart" style="margin-top:16px">
      <div class="api-etiket">GET /api/alanlar/${id}</div>
      <h2>${guvenli(alan.ad)}</h2>
      ${alan.konular.map((k) => `
        <div class="konu-satir" data-konu="${k.id}">
          <div>
            <div class="konu-baslik"><strong>${k.sira}.</strong> ${guvenli(k.baslik)}</div>
            <div class="meta">${guvenli(k.ozet)}</div>
          </div>
          <div class="meta">${k.tamamlandi ? '<span class="tik">✓</span>' : `~${k.tahminiDakika} dk`}</div>
        </div>`).join("")}
    </div>
    <div id="konuDetay"></div>`;

  detay.querySelectorAll("[data-konu]").forEach((satir) =>
    satir.addEventListener("click", () => konuDetayiniGoster(Number(satir.dataset.konu))));
}

async function konuDetayiniGoster(id) {
  const konu = await api(`/api/konular/${id}`);
  const kutu = document.getElementById("konuDetay");
  kutu.innerHTML = `<div class="api-etiket">GET /api/konular/${id}</div>` + konuKartiHtml(konu);
  konuKartiOlaylari(kutu, async () => {
    await alanlariYukle();
    await konuDetayiniGoster(id);
    await bugunuYukle();
  });
  kutu.scrollIntoView({ behavior: "smooth" });
}

// --- 📖 Sözlük --------------------------------------------------------------
let aramaZamanlayici;
document.getElementById("aramaKutusu").addEventListener("input", (e) => {
  // DEBOUNCE: Her tuşta istek atmak yerine, yazmayı bırakınca 300 ms sonra tek istek at.
  clearTimeout(aramaZamanlayici);
  aramaZamanlayici = setTimeout(() => sozlukAra(e.target.value), 300);
});

async function sozlukAra(kelime) {
  // encodeURIComponent: Boşluk, Türkçe karakter gibi şeyleri URL'ye uygun hale getirir.
  const terimler = await api(`/api/terimler?ara=${encodeURIComponent(kelime)}`);
  const sonuc = document.getElementById("sozlukSonuc");
  sonuc.innerHTML = `<p class="meta">${terimler.length} terim bulundu</p>` + (terimler.length === 0 ? "" : `
    <div class="kart">
      ${terimler.map((t) => `
        <div class="terim">
          <div class="terim-ad">${guvenli(t.ad)}</div>
          <div>${guvenli(t.tanim)}</div>
          ${t.ornek ? `<div class="terim-ornek">${guvenli(t.ornek)}</div>` : ""}
          <div class="terim-kaynak">${guvenli(t.alanAdi)} → ${guvenli(t.konuBaslik)}</div>
        </div>`).join("")}
    </div>`);
}

// --- Başlangıç: sayfa açılınca bugünü yükle ---------------------------------
bugunuYukle();
