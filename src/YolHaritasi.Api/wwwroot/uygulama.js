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

      ${konu.dersNotu ? `
        <details class="ders-notu" open>
          <summary>📘 Ders Notu</summary>
          ${markdown(konu.dersNotu)}
        </details>` : ""}

      ${konu.kaynaklar.length ? `
        <h3>🔗 Kaynaklar (detaylı okuma)</h3>
        ${konu.kaynaklar.map((k) => `
          <div class="kaynak">
            <span class="site">${guvenli(k.site)}</span>
            <a href="${guvenli(k.url)}" target="_blank" rel="noopener">${guvenli(k.baslik)}</a>
          </div>`).join("")}` : ""}

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

// --- 🧪 Projeler -------------------------------------------------------------
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
        <h3>Adımlar</h3>
        ${p.adimlar.map((a) => `
          <label class="adim ${a.tamamlandi ? "bitti" : ""}">
            <input type="checkbox" data-adim="${a.id}" ${a.tamamlandi ? "checked" : ""}>
            <div>
              <div class="adim-metin"><strong>${a.sira}.</strong> ${guvenli(a.aciklama)}</div>
              ${a.ipucu ? `<div class="ipucu">💡 ${guvenli(a.ipucu)}</div>` : ""}
            </div>
          </label>`).join("")}
        <div class="api-etiket" style="margin-top:8px">PATCH /api/projeler/adimlar/{id}</div>
      </div>`;
  }).join("");

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

// --- Başlangıç: sayfa açılınca bugünü yükle ---------------------------------
bugunuYukle();
