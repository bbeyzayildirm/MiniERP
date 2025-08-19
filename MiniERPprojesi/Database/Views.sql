CREATE VIEW v_adresler AS
SELECT 
    a.AdresID,
    a.IlceID,
    ilce.IlceAdi,
    il.IlAdi,
    a.Mahalle,
    a.Sokak,
    a.BinaNo,
    a.DaireNo,
    a.PostaKodu,
    a.Aciklama,
    il.IlAdi + ' / ' + ilce.IlceAdi + ', ' + 
    a.Mahalle + ' Mah., ' + 
    a.Sokak + ' Sok., No:' + 
    CAST(a.BinaNo AS VARCHAR) + '/' + 
    CAST(a.DaireNo AS VARCHAR) + ', ' + 
    a.PostaKodu AS TamAdres
FROM Adresler a
INNER JOIN Ilceler ilce ON a.IlceID = ilce.IlceID
INNER JOIN Iller il ON ilce.IlID = il.IlID;

CREATE VIEW v_depolar AS
SELECT 
    d.DepoID,
    d.DepoAdi,
    a.AdresID,
    il.IlAdi,
    ilc.IlceAdi,
    a.Mahalle,
    a.Sokak,
    a.BinaNo,
    a.DaireNo,
    a.PostaKodu,
    a.Mahalle + ' Mah. ' + a.Sokak + ' Sok. No:' + 
        CAST(a.BinaNo AS VARCHAR) + '/' + CAST(a.DaireNo AS VARCHAR) + ' ' + a.PostaKodu AS Adres,
    d.OlusturanKullaniciID,
    k.Ad + ' ' + k.Soyad AS KullaniciAdi
FROM Depolar d
INNER JOIN Adresler a ON d.AdresID = a.AdresID
INNER JOIN Ilceler ilc ON a.IlceID = ilc.IlceID
INNER JOIN Iller il ON il.IlID = ilc.IlID
INNER JOIN Kullanicilar k ON d.OlusturanKullaniciID = k.KullaniciID;

CREATE VIEW v_depostok AS
SELECT 
    ds.DepoID,
    d.DepoAdi,
    ds.UrunID,
    u.UrunAdi,
    ds.Miktar
FROM DepoStok ds
INNER JOIN Depolar d ON ds.DepoID = d.DepoID
INNER JOIN Urunler u ON ds.UrunID = u.UrunID;

CREATE VIEW v_faturalar AS
SELECT 
    f.FaturaID,
    f.FaturaNo,
    f.FaturaTarihi,
    f.SiparisID,
    s.SiparisNo,
    f.ToplamTutar,
    f.OdemeDurumu,
    k.Ad + ' ' + k.Soyad AS KullaniciAdi
FROM Faturalar f
INNER JOIN Siparisler s ON f.SiparisID = s.SiparisID
INNER JOIN Kullanicilar k ON f.KullaniciID = k.KullaniciID

CREATE VIEW v_kullanicilar AS
SELECT 
    k.KullaniciID,
    k.KullaniciAdi,
    k.Ad,
    k.Soyad,
    k.Email,
    k.ProfilFotoUrl,
    r.RolAdi
FROM 
    Kullanicilar k
INNER JOIN 
    Roller r ON k.RolID = r.RolID;

CREATE VIEW v_musteriler AS
SELECT 
    m.MusteriID,
    m.Ad,
    m.Soyad,
    m.Email,
    m.Telefon,
    m.KimlikNo,
    m.MusteriTuru,
    a.AdresID,
    il.IlAdi,
    ilc.IlceAdi,
    a.Mahalle,
    a.Sokak,
    a.BinaNo,
    a.DaireNo,
    a.PostaKodu,
    a.Mahalle + ' Mah. ' + a.Sokak + ' Sok. No:' + 
        CAST(a.BinaNo AS VARCHAR) + '/' + CAST(a.DaireNo AS VARCHAR) + ' ' + a.PostaKodu AS Adres,
    k.Ad + ' ' + k.Soyad AS OlusturanKullanici
FROM Musteriler m
INNER JOIN Adresler a ON m.AdresID = a.AdresID
INNER JOIN Ilceler ilc ON a.IlceID = ilc.IlceID
INNER JOIN Iller il ON il.IlID = ilc.IlID
INNER JOIN Kullanicilar k ON m.OlusturanKullaniciID = k.KullaniciID;

CREATE VIEW v_siparisdetay AS
SELECT 
    s.SiparisID,
    s.SiparisNo,
    s.SiparisTarihi,
    s.SiparisDurumu,
    s.FaturalandiMi,

    m.Ad + ' ' + m.Soyad AS MusteriAdi,
    m.Email,
    m.Telefon,
    m.MusteriTuru,

    il.IlAdi,
    ilc.IlceAdi,

    a.Mahalle,
    a.Sokak,
    a.BinaNo,
    a.DaireNo,
    a.PostaKodu,
    a.Mahalle + ' Mah. ' + a.Sokak + ' Sok. No:' + 
        CAST(a.BinaNo AS VARCHAR) + '/' + CAST(a.DaireNo AS VARCHAR) + ' ' + a.PostaKodu AS Adres,

    u.UrunAdi,
    d.DepoAdi,
    sk.Adet,
    sk.BirimFiyat,
    sk.IptalEdildi

FROM SiparisKalemleri sk
INNER JOIN Siparisler s ON s.SiparisID = sk.SiparisID
INNER JOIN Urunler u ON sk.UrunID = u.UrunID
INNER JOIN Depolar d ON sk.DepoID = d.DepoID
INNER JOIN Musteriler m ON s.MusteriID = m.MusteriID
INNER JOIN Adresler a ON s.AdresID = a.AdresID
INNER JOIN Ilceler ilc ON a.IlceID = ilc.IlceID
INNER JOIN Iller il ON ilc.IlID = il.IlID;

CREATE VIEW v_siparisler AS
SELECT 
    s.SiparisID,
    s.SiparisNo,
    s.SiparisTarihi,
    s.SiparisDurumu,
    s.FaturalandiMi,
    m.Ad + ' ' + m.Soyad AS MusteriAdi
FROM Siparisler s
INNER JOIN Musteriler m ON s.MusteriID = m.MusteriID;

CREATE VIEW v_stokhareketleri AS
SELECT 
    sh.StokHareketID,       
    sh.DepoID,            
    d.DepoAdi,      

    sh.UrunID,           
    u.UrunAdi,    

    sh.Miktar,      -- Giriş/çıkış yapılan ürün miktarı
    sh.BirimFiyat,  -- O ürün için işlem anındaki birim fiyat
    (sh.Miktar * sh.BirimFiyat) AS ToplamTutar, 

    sh.HareketTipi,          -- 1 = Giriş, 2 = Çıkış
    CASE 
        WHEN sh.HareketTipi = 1 THEN 'Giriş'      -- 1 için metinsel karşılığı
        WHEN sh.HareketTipi = 2 THEN 'Çıkış'      -- 2 için metinsel karşılığı
        ELSE 'Bilinmiyor'   
    END AS HareketTipiAdi,

    sh.HareketTarihi,  

    sh.KullaniciID,      
    k.Ad + ' ' + k.Soyad AS KullaniciAdi, 

    sh.IptalKaynakli       -- Bu hareket bir iptalden mi kaynaklı (True/False)
FROM 
    StokHareketleri sh    
JOIN Depolar d ON sh.DepoID = d.DepoID      
JOIN Urunler u ON sh.UrunID = u.UrunID      
JOIN Kullanicilar k ON sh.KullaniciID = k.KullaniciID 

CREATE VIEW v_urunler AS
SELECT 
    u.UrunID,
    u.UrunAdi,
    u.Aciklama,
    u.Fiyat,
    u.KategoriID,
    k.KategoriAdi,
    u.ResimYolu,
    u.OlusturanKullaniciID
FROM Urunler u
INNER JOIN Kategoriler k ON u.KategoriID = k.KategoriID;