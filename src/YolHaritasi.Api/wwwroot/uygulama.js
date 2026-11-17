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
  // Tırnakları da kaçırıyoruz; çünkü bazı değerler href="..." gibi HTML özniteliklerinin içine giriyor.
  return d.innerHTML.replace(/"/g, "&quot;").replace(/'/g, "&#39;");
}

// --- Basit Markdown → HTML çevirici (ders notları için) -----------------------
// Önce TÜM metni guvenli() ile kaçırıyoruz, sonra sadece kendi ürettiğimiz etiketleri ekliyoruz.
// Böylece veritabanındaki metin asla HTML/JavaScript olarak çalışamaz.
function markdown(metin) {
  const satirlar = guvenli(metin).split("\n");
  let html = "", kodda = false, kod = [], liste = null;

  const listeyiKapat = () => { if (liste) { html += `</${liste}>`; liste = null; } };
  const satirIci = (s) => s
    .replace(/`([^`]+)`/g, "<code>$1</code>")
    .replace(/\*\*([^*]+)\*\*/g, "<strong>$1</strong>");

  for (const satir of satirlar) {
    if (satir.startsWith("```")) {
      if (kodda) { html += `<pre><code>${kod.join("\n")}</code></pre>`; kod = []; }
      else listeyiKapat();
      kodda = !kodda;
      continue;
    }
    if (kodda) { kod.push(satir); continue; }

    if (satir.startsWith("### ")) { listeyiKapat(); html += `<h5>${satirIci(satir.slice(4))}</h5>`; }
    else if (satir.startsWith("## ")) { listeyiKapat(); html += `<h4>${satirIci(satir.slice(3))}</h4>`; }
    else if (/^- /.test(satir)) {
      if (liste !== "ul") { listeyiKapat(); html += "<ul>"; liste = "ul"; }
      html += `<li>${satirIci(satir.slice(2))}</li>`;
    }
    else if (/^\d+\. /.test(satir)) {
      if (liste !== "ol") { listeyiKapat(); html += "<ol>"; liste = "ol"; }
      html += `<li>${satirIci(satir.replace(/^\d+\. /, ""))}</li>`;
    }
    else if (satir.trim() === "") listeyiKapat();
    else { listeyiKapat(); html += `<p>${satirIci(satir)}</p>`; }
  }
  listeyiKapat();
  return html;
}

// --- Terimin gerçek kullanım örneği (Sözlük ve konu rehberinde ortak) ---------
const KOD_DILLERI = { csharp: "C#", sql: "SQL", powershell: "PowerShell", http: "HTTP isteği", json: "JSON", text: "Örnek" };

function terimOrnekHtml(t) {
  if (!t.kullanim && !t.kodOrnegi) return "";
  return `
    ${t.kullanim ? `<div class="kullanim"><strong>🧭 Nerede kullanılır?</strong> ${guvenli(t.kullanim)}</div>` : ""}
    ${t.kodOrnegi ? `
      <div class="kod-kutu">
        <div class="kod-ust">
          <span>💻 Örnek kullanım · ${guvenli(KOD_DILLERI[t.kodDili] ?? t.kodDili ?? "")}</span>
          <button class="kopyala" type="button">Kopyala</button>
        </div>
        <pre><code>${guvenli(t.kodOrnegi)}</code></pre>
      </div>` : ""}`;
}

// "Kopyala" düğmeleri: Sayfadaki tüm kopyala düğmelerini tek bir dinleyiciyle yönetiyoruz (event delegation).
document.addEventListener("click", async (e) => {
  const dugme = e.target.closest(".kopyala");
  if (!dugme) return;
  e.stopPropagation();
  const kod = dugme.closest(".kod-kutu").querySelector("code").textContent;
  try {
    await navigator.clipboard.writeText(kod);
    dugme.textContent = "Kopyalandı ✓";
  } catch {
    dugme.textContent = "Kopyalanamadı";
  }
  setTimeout(() => (dugme.textContent = "Kopyala"), 1500);
});

// --- Sekmeler ----------------------------------------------------------------
document.querySelectorAll(".sekme").forEach((dugme) => {
  dugme.addEventListener("click", () => {
    document.querySelectorAll(".sekme").forEach((d) => d.classList.remove("aktif"));
    document.querySelectorAll(".panel").forEach((p) => p.classList.remove("aktif"));
    dugme.classList.add("aktif");
    document.getElementById(dugme.dataset.sekme).classList.add("aktif");

    if (dugme.dataset.sekme === "alanlar") alanlariYukle();
    if (dugme.dataset.sekme === "sozluk") sozlukAra("");
    if (dugme.dataset.sekme === "projeler") projeleriYukle();
    if (dugme.dataset.sekme === "veritabani") veritabaniniYukle();
  });
});

// --- KONU REHBERİ -------------------------------------------------------------
// Bir konuyu tek seferde değil, 5 adımda gösteriyoruz: Oku → Kaynaklar → Terimler → Görevler → Bitir.
// Her adımda ekranda sadece o adımın içeriği ve "şimdi ne yapmalısın" açıklaması var.
const ADIMLAR = [
  { anahtar: "oku", ad: "Oku", ikon: "📘" },
  { anahtar: "kaynak", ad: "Kaynaklar", ikon: "🔗" },
  { anahtar: "terim", ad: "Terimler", ikon: "📖" },
  { anahtar: "gorev", ad: "Görevler", ikon: "🛠️" },
  { anahtar: "bitir", ad: "Bitir", ikon: "✅" },
];

// Hangi konuda hangi adımda kaldığını tarayıcıda hatırla (sayfayı yenileyince kaybolmasın).
// localStorage bazı tarayıcı ayarlarında çalışmayabilir; bu yüzden try/catch içinde.
function kayitliAdim(konuId) {
  try { return Number(localStorage.getItem(`adim-${konuId}`)) || 0; } catch { return 0; }
}
function adimiKaydet(konuId, adim) {
  try { localStorage.setItem(`adim-${konuId}`, adim); } catch { }
}

/**
 * Konu rehberini verilen kutunun içine çizer.
 * tamamlaninca: "Konuyu tamamladım"a basıldıktan sonra çağrılacak fonksiyon.
 */
function konuRehberi(kutu, konu, tamamlaninca) {
  let adim = Math.min(kayitliAdim(konu.id), ADIMLAR.length - 1);

  function ciz() {
    adimiKaydet(konu.id, adim);
    const tamam = konu.tamamlanmaTarihi !== null;
    const yapilanGorev = konu.gorevler.filter((g) => g.tamamlandi).length;

    kutu.innerHTML = `
      <div class="kart rehber">
        <div class="meta">
          <span class="rozet">${guvenli(konu.alanAdi)} · ${konu.sira}. konu</span>
          <span class="rozet">⏱️ ~${konu.tahminiDakika} dk</span>
          ${tamam ? '<span class="rozet tamam">✓ Tamamlandı</span>' : ""}
        </div>
        <h2>${guvenli(konu.baslik)}</h2>
        <p class="meta">${guvenli(konu.ozet)}</p>

        <ol class="adim-cubugu">
          ${ADIMLAR.map((a, i) => `
            <li class="${i === adim ? "simdi" : i < adim ? "gecti" : ""}">
              <button data-git="${i}"><span>${i < adim ? "✓" : i + 1}</span>${a.ikon} ${a.ad}</button>
            </li>`).join("")}
        </ol>

        <div class="adim-icerik">${adimIcerigi(ADIMLAR[adim].anahtar, konu, tamam, yapilanGorev)}</div>

        <div class="rehber-alt">
          <button class="dugme ikincil" data-onceki ${adim === 0 ? "disabled" : ""}>← Geri</button>
          <span class="meta">Adım ${adim + 1} / ${ADIMLAR.length}</span>
          ${adim < ADIMLAR.length - 1
            ? `<button class="dugme" data-sonraki>İleri: ${ADIMLAR[adim + 1].ikon} ${ADIMLAR[adim + 1].ad} →</button>`
            : `<span></span>`}
        </div>
      </div>`;

    kutu.querySelectorAll("[data-git]").forEach((b) =>
      b.addEventListener("click", () => { adim = Number(b.dataset.git); ciz(); ustaKaydir(); }));
    kutu.querySelector("[data-onceki]")?.addEventListener("click", () => { adim--; ciz(); ustaKaydir(); });
    kutu.querySelector("[data-sonraki]")?.addEventListener("click", () => { adim++; ciz(); ustaKaydir(); });

    // Terim kartları: tıklayınca tanım açılır
    // Sadece başlığa tıklayınca aç/kapa; böylece kodu seçip kopyalarken kart kapanmaz
    kutu.querySelectorAll(".terim-ust").forEach((ust) =>
      ust.addEventListener("click", () => ust.closest(".terim-kart").classList.toggle("acik")));

    // Görev kutuları: PATCH /api/gorevler/{id}
    kutu.querySelectorAll("[data-gorev]").forEach((kutucuk) =>
      kutucuk.addEventListener("change", async () => {
        await api(`/api/gorevler/${kutucuk.dataset.gorev}`, {
          method: "PATCH",
          body: JSON.stringify({ tamamlandi: kutucuk.checked }),
        });
        konu.gorevler.find((g) => g.id === Number(kutucuk.dataset.gorev)).tamamlandi = kutucuk.checked;
        ciz();
      }));

    // Konuyu tamamla: POST /api/konular/{id}/tamamla
    kutu.querySelector("[data-tamamla]")?.addEventListener("click", async () => {
      await api(`/api/konular/${konu.id}/tamamla`, { method: "POST" });
      adimiKaydet(konu.id, 0);
      await tamamlaninca();
    });
  }

  function ustaKaydir() {
    kutu.scrollIntoView({ behavior: "smooth", block: "start" });
  }

  ciz();
}

function adimIcerigi(anahtar, konu, tamam, yapilanGorev) {
  switch (anahtar) {
    case "oku":
      return konu.dersNotu
        ? `<p class="yapilacak">👉 <strong>Şimdi:</strong> Aşağıdaki ders notunu baştan sona oku. Kod örneklerini satır satır anlamaya çalış. Acele etme.</p>
           <div class="ders-notu">${markdown(konu.dersNotu)}</div>`
        : `<p class="yapilacak">👉 Bu konunun ders notu henüz yazılmadı. <strong>İleri</strong>'ye basıp kaynaklardan oku.</p>`;

    case "kaynak":
      return konu.kaynaklar.length
        ? `<p class="yapilacak">👉 <strong>Şimdi:</strong> Bu sayfaları aç ve oku (yeni sekmede açılır). Ders notundaki konuları farklı örneklerle pekiştirirsin. W3Schools sayfalarındaki <em>"Try it Yourself"</em> düğmesiyle kodu tarayıcıda deneyebilirsin.</p>
           <ul class="kaynak-listesi">
             ${konu.kaynaklar.map((k) => `
               <li><a href="${guvenli(k.url)}" target="_blank" rel="noopener">${guvenli(k.baslik)} ↗</a>
                   <span class="site">${guvenli(k.site)}</span></li>`).join("")}
           </ul>`
        : `<p class="yapilacak">👉 Bu konu için ek kaynak yok. <strong>İleri</strong>'ye geç.</p>`;

    case "terim":
      return `<p class="yapilacak">👉 <strong>Şimdi:</strong> Her terimin adını oku ve anlamını <strong>önce kendin hatırlamaya çalış</strong>. Sonra terime tıkla: tanımı, gerçek projelerde nerede kullanıldığını ve örnek kodu gör.</p>
        <div class="terim-izgara">
          ${konu.terimler.map((t) => `
            <div class="terim-kart">
              <div class="terim-ust">
                <div class="terim-ad">${guvenli(t.ad)}</div>
                <div class="terim-ipucu">Tanımı ve örneği görmek için tıkla ▾</div>
              </div>
              <div class="terim-arka">
                <div class="terim-tanim">${guvenli(t.tanim)}</div>
                ${terimOrnekHtml(t)}
              </div>
            </div>`).join("")}
        </div>`;

    case "gorev":
      return `<p class="yapilacak">👉 <strong>Şimdi:</strong> Bu görevleri <strong>kendi bilgisayarında</strong> yap. Her birini bitirince kutusunu işaretle.</p>
        <div class="gorev-listesi">
          ${konu.gorevler.map((g) => `
            <label class="gorev ${g.tamamlandi ? "bitti" : ""}">
              <input type="checkbox" data-gorev="${g.id}" ${g.tamamlandi ? "checked" : ""}>
              <span>${guvenli(g.aciklama)}</span>
            </label>`).join("")}
        </div>
        <p class="meta">${yapilanGorev} / ${konu.gorevler.length} görev yapıldı</p>
        <div class="api-etiket">PATCH /api/gorevler/{id}</div>`;

    case "bitir":
      if (tamam)
        return `<p class="yapilacak">✅ Bu konuyu zaten tamamladın. Sıradaki konu için <strong>📅 Bugün</strong> sekmesine dön.</p>`;
      return `<p class="yapilacak">👉 <strong>Son adım.</strong> Kendine şunları sor:</p>
        <ul class="kontrol-listesi">
          <li>Ders notunu okudum ve kod örneklerini anladım.</li>
          <li>Terimlerin çoğunu tanıma bakmadan açıklayabiliyorum.</li>
          <li>Görevleri yaptım (${yapilanGorev} / ${konu.gorevler.length}).</li>
        </ul>
        <p>Hepsine "evet" diyorsan konuyu tamamla. Takıldığın yer varsa önce geri dönüp tekrar et.</p>
        <button class="dugme buyuk" data-tamamla>✓ Konuyu tamamladım</button>
        <div class="api-etiket">POST /api/konular/${konu.id}/tamamla</div>`;
  }
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
    alan.innerHTML = `<h2 class="bolum-baslik">Bugünün konusu</h2><div id="bugunRehber"></div>`;
    konuRehberi(document.getElementById("bugunRehber"), bugun.konu, async () => {
      await bugunuYukle();
      document.getElementById("bugunIcerik").insertAdjacentHTML("afterbegin",
        `<div class="bilgi-kutusu basari">🎉 Tebrikler, konuyu tamamladın! İşte sıradaki konun.</div>`);
    });
  } catch (e) {
    alan.innerHTML = `<div class="kart">⚠️ ${guvenli(e.message)}</div>`;
  }
}

function genelIlerlemeyiGoster(yuzde) {
  document.getElementById("genelYuzde").textContent = `%${yuzde}`;
  document.getElementById("genelCubuk").style.width = `${yuzde}%`;
}

// --- 🗺️ Tüm Konular ------------------------------------------------------------
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
      <p class="meta">Bir konuya tıkla, aşağıda açılsın.</p>
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
  konuRehberi(kutu, konu, async () => {
    await alanlariYukle();
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
  // Az sonuç varsa örnekler açık gelsin; çok sonuç varsa (ör. tüm liste) kapalı, tıklayınca açılsın.
  const acik = terimler.length <= 5;
  sonuc.innerHTML = `<p class="meta">${terimler.length} terim bulundu</p>` + (terimler.length === 0 ? "" : `
    <div class="kart">
      ${terimler.map((t) => `
        <div class="terim">
          <div class="terim-ad">${guvenli(t.ad)}</div>
          <div class="terim-tanim">${guvenli(t.tanim)}</div>
          <div class="terim-kaynak">${guvenli(t.alanAdi)} → ${guvenli(t.konuBaslik)}</div>
          ${t.kodOrnegi || t.kullanim ? `
            <details class="terim-ornek-detay" ${acik ? "open" : ""}>
              <summary>💻 Gerçek kullanım örneği</summary>
              ${terimOrnekHtml(t)}
            </details>` : ""}
        </div>`).join("")}
    </div>`);
}

// --- 🧪 Projeler -------------------------------------------------------------
const acikProjeler = new Set();   // yeniden çizince açık olan projeler kapanmasın

async function projeleriYukle() {
  const projeler = await api("/api/projeler");
  const liste = document.getElementById("projeListesi");
  liste.innerHTML = projeler.map((p) => {
    const yuzde = p.adimSayisi === 0 ? 0 : Math.floor(p.tamamlananAdimSayisi * 100 / p.adimSayisi);
    return `
      <div class="kart">
        <div class="meta">
          <span class="rozet">${guvenli(p.alanAdi)}</span>
          <span class="rozet">${guvenli(p.zorluk)}</span>
          ${yuzde === 100 ? '<span class="rozet tamam">✓ Tamamlandı</span>' : ""}
        </div>
        <h2>${guvenli(p.baslik)}</h2>
        <p>${guvenli(p.aciklama)}</p>
        <div class="cubuk"><div class="dolgu" style="width:${yuzde}%"></div></div>
        <div class="meta">${p.tamamlananAdimSayisi} / ${p.adimSayisi} adım · %${yuzde}</div>
        <details class="proje-adimlar" ${acikProjeler.has(p.id) ? "open" : ""} data-proje="${p.id}">
        <summary>${p.tamamlananAdimSayisi === 0 ? "Projeye başla: adımları göster" : "Adımları göster"}</summary>
        ${p.adimlar.map((a) => `
          <label class="adim ${a.tamamlandi ? "bitti" : ""}">
            <input type="checkbox" data-adim="${a.id}" ${a.tamamlandi ? "checked" : ""}>
            <div>
              <div class="adim-metin"><strong>${a.sira}.</strong> ${guvenli(a.aciklama)}</div>
              ${a.ipucu ? `<div class="ipucu">💡 ${guvenli(a.ipucu)}</div>` : ""}
            </div>
          </label>`).join("")}
        </details>
        <div class="api-etiket" style="margin-top:8px">PATCH /api/projeler/adimlar/{id}</div>
      </div>`;
  }).join("");

  liste.querySelectorAll("[data-proje]").forEach((d) =>
    d.addEventListener("toggle", () => d.open ? acikProjeler.add(Number(d.dataset.proje)) : acikProjeler.delete(Number(d.dataset.proje))));

  liste.querySelectorAll("[data-adim]").forEach((kutu) =>
    kutu.addEventListener("change", async () => {
      await api(`/api/projeler/adimlar/${kutu.dataset.adim}`, {
        method: "PATCH",
        body: JSON.stringify({ tamamlandi: kutu.checked }),
      });
      await projeleriYukle();   // ilerleme çubuğunu güncellemek için listeyi yeniden çiz
    }));
}

// --- 🗄️ Veritabanı -----------------------------------------------------------
async function veritabaniniYukle() {
  const d = await api("/api/veritabani/durum");
  const kutu = document.getElementById("dbDurum");
  const renk = d.bagli ? "var(--basari)" : "#d9534f";

  kutu.innerHTML = `
    <div class="kart">
      <h2><span class="durum-nokta" style="background:${renk}"></span>${d.bagli ? "Bağlantı kuruldu" : "Bağlantı yok"}</h2>
      <dl class="bilgi-izgara">
        <dt>Sağlayıcı</dt><dd>${guvenli(d.saglayici)}</dd>
        <dt>Sunucu</dt><dd><code>${guvenli(d.sunucu)}</code></dd>
        <dt>Veritabanı</dt><dd><code>${guvenli(d.veritabani)}</code></dd>
        <dt>Sürüm</dt><dd>${guvenli(d.sqlServerSurumu ?? "-")}</dd>
        <dt>Migration'lar</dt><dd>${d.uygulananMigrationlar.map((m) => `<code>${guvenli(m)}</code>`).join("<br>") || "-"}</dd>
      </dl>
      ${d.hata ? `<div class="hata">⚠️ ${guvenli(d.hata)}</div>` : ""}
      <p class="meta" style="margin-top:12px">Bu bilgiler <code>appsettings.Development.json</code> içindeki connection string'den gelir. Şifre gibi gizli kısımlar asla ekrana basılmaz.</p>
    </div>

    ${d.tablolar.length ? `
    <div class="kart">
      <h2>📋 Tablolar</h2>
      <p class="meta">Bir tabloya tıkla: sütunları, tipleri, birincil anahtarı (🔑) ve bağlı olduğu tabloyu (🔗) gör.</p>
      ${d.tablolar.map((t) => `
        <details class="tablo-detay">
          <summary><strong>${guvenli(t.ad)}</strong> <span class="meta">· ${t.kayitSayisi} kayıt · ${t.sutunlar.length} sütun</span></summary>
          <div class="tablo-kutu">
            <table>
              <tr><th>Sütun</th><th>Tip</th><th>NULL olabilir</th><th>Anahtar</th></tr>
              ${t.sutunlar.map((s) => `
                <tr>
                  <td>${s.birincilAnahtar ? "🔑 " : ""}${guvenli(s.ad)}</td>
                  <td><code>${guvenli(s.tip)}</code></td>
                  <td>${s.bosOlabilir ? "Evet" : "Hayır"}</td>
                  <td>${s.birincilAnahtar ? "Primary Key" : s.bagliTablo ? `🔗 Foreign Key → ${guvenli(s.bagliTablo)}` : ""}</td>
                </tr>`).join("")}
            </table>
          </div>
        </details>`).join("")}
    </div>` : ""}`;
}

const ORNEK_SORGULAR = {
  "Tüm alanlar": "SELECT * FROM Alanlar ORDER BY Sira",
  "Alan başına konu": "SELECT a.Ad, COUNT(*) AS KonuSayisi\nFROM Alanlar a\nJOIN Konular k ON k.AlanId = a.Id\nGROUP BY a.Ad",
  "Tamamlanan konular": "SELECT a.Ad AS Alan, k.Baslik, k.TamamlanmaTarihi\nFROM Konular k\nJOIN Alanlar a ON a.Id = k.AlanId\nWHERE k.TamamlanmaTarihi IS NOT NULL\nORDER BY k.TamamlanmaTarihi",
  "Terim ara (LIKE)": "SELECT t.Ad, t.Tanim, k.Baslik AS Konu\nFROM Terimler t\nJOIN Konular k ON k.Id = t.KonuId\nWHERE t.Ad LIKE '%Key%'",
  "Kaynağı olmayan konular": "SELECT k.Id, k.Baslik\nFROM Konular k\nLEFT JOIN Kaynaklar x ON x.KonuId = k.Id\nWHERE x.Id IS NULL",
  "En uzun konular (TOP)": "SELECT TOP 5 Baslik, TahminiDakika\nFROM Konular\nORDER BY TahminiDakika DESC",
  "Proje ilerlemesi": "SELECT p.Baslik,\n       COUNT(*) AS AdimSayisi,\n       SUM(CASE WHEN a.Tamamlandi = 1 THEN 1 ELSE 0 END) AS Tamamlanan\nFROM Projeler p\nJOIN ProjeAdimlari a ON a.ProjeId = p.Id\nGROUP BY p.Baslik",
};

const ornekKutu = document.getElementById("ornekSorgular");
for (const [ad, sql] of Object.entries(ORNEK_SORGULAR)) {
  const b = document.createElement("button");
  b.textContent = ad;
  b.addEventListener("click", () => { document.getElementById("sqlKutusu").value = sql; sorguyuCalistir(); });
  ornekKutu.appendChild(b);
}

document.getElementById("sorguCalistir").addEventListener("click", sorguyuCalistir);

async function sorguyuCalistir() {
  const sonuc = document.getElementById("sorguSonuc");
  const sql = document.getElementById("sqlKutusu").value;
  sonuc.innerHTML = '<p class="meta">Çalışıyor…</p>';
  try {
    const s = await api("/api/veritabani/sorgu", { method: "POST", body: JSON.stringify({ sql }) });
    sonuc.innerHTML = `
      <p class="meta">${s.satirSayisi} satır · ${s.sureMs} ms${s.kesildi ? " · ilk 200 satır gösteriliyor" : ""}</p>
      <div class="tablo-kutu">
        <table>
          <tr>${s.sutunlar.map((c) => `<th>${guvenli(c)}</th>`).join("")}</tr>
          ${s.satirlar.map((r) => `<tr>${r.map((v) => v === null ? '<td class="null">NULL</td>' : `<td>${guvenli(v)}</td>`).join("")}</tr>`).join("")}
        </table>
      </div>`;
  } catch (e) {
    sonuc.innerHTML = `<div class="hata">⚠️ ${guvenli(e.message)}</div>`;
  }
}

// --- Hoş geldin kutusu ve geliştirici modu ------------------------------------
function ayarOku(ad) { try { return localStorage.getItem(ad); } catch { return null; } }
function ayarYaz(ad, deger) { try { localStorage.setItem(ad, deger); } catch { } }

const hosgeldin = document.getElementById("hosgeldin");
hosgeldin.hidden = ayarOku("hosgeldinKapali") === "1";
document.getElementById("hosgeldinKapat").addEventListener("click", () => {
  hosgeldin.hidden = true;
  ayarYaz("hosgeldinKapali", "1");
});

// Geliştirici modu: Açıkken her bölümün çağırdığı API adresi (mavi etiketler) görünür.
const gelistiriciKutu = document.getElementById("gelistiriciModu");
gelistiriciKutu.checked = ayarOku("gelistiriciModu") === "1";
document.body.classList.toggle("gelistirici", gelistiriciKutu.checked);
gelistiriciKutu.addEventListener("change", () => {
  document.body.classList.toggle("gelistirici", gelistiriciKutu.checked);
  ayarYaz("gelistiriciModu", gelistiriciKutu.checked ? "1" : "0");
});

// --- Başlangıç: sayfa açılınca bugünü yükle ---------------------------------
bugunuYukle();
