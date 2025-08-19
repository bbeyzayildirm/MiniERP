--  Mini ERP Database Tables

CREATE TABLE [dbo].[Iller] (
    [IlID]  INT            IDENTITY (1, 1) NOT NULL,
    [IlAdi] NVARCHAR (100) NOT NULL,
    PRIMARY KEY CLUSTERED ([IlID] ASC)
);

CREATE TABLE [dbo].[Ilceler] (
    [IlceID]  INT            IDENTITY (1, 1) NOT NULL,
    [IlceAdi] NVARCHAR (100) NOT NULL,
    [IlID]    INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([IlceID] ASC),
    CONSTRAINT [FK_Ilceler_Iller] FOREIGN KEY ([IlID]) REFERENCES [dbo].[Iller] ([IlID])
);

CREATE TABLE [dbo].[Adresler] (
    [AdresID]   INT            IDENTITY (1, 1) NOT NULL,
    [IlceID]    INT            NOT NULL,
    [Mahalle]   NVARCHAR (100) NULL,
    [Sokak]     NVARCHAR (100) NULL,
    [BinaNo]    NVARCHAR (50)  NULL,
    [DaireNo]   NVARCHAR (50)  NULL,
    [PostaKodu] NVARCHAR (20)  NULL,
    [Aciklama]  NVARCHAR (255) NULL,
    PRIMARY KEY CLUSTERED ([AdresID] ASC),
    CONSTRAINT [FK_Adresler_Ilceler] FOREIGN KEY ([IlceID]) REFERENCES [dbo].[Ilceler] ([IlceID])
);

CREATE TABLE [dbo].[Roller] (
    [RolID]  INT            IDENTITY (1, 1) NOT NULL,
    [RolAdi] NVARCHAR (100) NOT NULL,
    PRIMARY KEY CLUSTERED ([RolID] ASC)
);

CREATE TABLE [dbo].[Kategoriler] (
    [KategoriID]  INT            IDENTITY (1, 1) NOT NULL,
    [KategoriAdi] NVARCHAR (150) NOT NULL,
    PRIMARY KEY CLUSTERED ([KategoriID] ASC)
);

CREATE TABLE [dbo].[Kategoriler] (
    [KategoriID]  INT            IDENTITY (1, 1) NOT NULL,
    [KategoriAdi] NVARCHAR (150) NOT NULL,
    PRIMARY KEY CLUSTERED ([KategoriID] ASC)
);

CREATE TABLE [dbo].[Kullanicilar] (
    [KullaniciID]   INT            IDENTITY (1, 1) NOT NULL,
    [KullaniciAdi]  NVARCHAR (100) NOT NULL,
    [SifreHash]     NVARCHAR (256) NOT NULL,
    [Email]         NVARCHAR (255) NOT NULL,
    [RolID]         INT            NOT NULL,
    [Ad]            NVARCHAR (50)  NULL,
    [Soyad]         NVARCHAR (50)  NULL,
    [ProfilFotoUrl] NVARCHAR (500) NULL,
    PRIMARY KEY CLUSTERED ([KullaniciID] ASC),
    CONSTRAINT [FK_Kullanicilar_Roller] FOREIGN KEY ([RolID]) REFERENCES [dbo].[Roller] ([RolID])
);

CREATE TABLE [dbo].[Depolar] (
    [DepoID]               INT            IDENTITY (1, 1) NOT NULL,
    [DepoAdi]              NVARCHAR (150) NOT NULL,
    [AdresID]              INT            NOT NULL,
    [OlusturanKullaniciID] INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([DepoID] ASC),
    CONSTRAINT [FK_Depolar_Adresler] FOREIGN KEY ([AdresID]) REFERENCES [dbo].[Adresler] ([AdresID]),
    CONSTRAINT [FK_Depolar_Kullanicilar] FOREIGN KEY ([OlusturanKullaniciID]) REFERENCES [dbo].[Kullanicilar] ([KullaniciID])
);

CREATE TABLE [dbo].[Urunler] (
    [UrunID]               INT             IDENTITY (1, 1) NOT NULL,
    [UrunAdi]              NVARCHAR (200)  NOT NULL,
    [Aciklama]             NVARCHAR (MAX)  NULL,
    [Fiyat]                DECIMAL (18, 2) NOT NULL,
    [KategoriID]           INT             NOT NULL,
    [OlusturanKullaniciID] INT             NOT NULL,
    [ResimYolu]            NVARCHAR (500)  NULL,
    PRIMARY KEY CLUSTERED ([UrunID] ASC),
    CONSTRAINT [FK_Urunler_Kategoriler] FOREIGN KEY ([KategoriID]) REFERENCES [dbo].[Kategoriler] ([KategoriID]),
    CONSTRAINT [FK_Urunler_Kullanicilar] FOREIGN KEY ([OlusturanKullaniciID]) REFERENCES [dbo].[Kullanicilar] ([KullaniciID])
);

CREATE TABLE [dbo].[Musteriler] (
    [MusteriID]            INT            IDENTITY (1, 1) NOT NULL,
    [Email]                NVARCHAR (255) NULL,
    [Telefon]              NVARCHAR (50)  NULL,
    [AdresID]              INT            NOT NULL,
    [KimlikNo]             NVARCHAR (50)  NULL,
    [OlusturanKullaniciID] INT            NOT NULL,
    [MusteriTuru]          NVARCHAR (20)  DEFAULT ('Bireysel') NOT NULL,
    [Ad]                   NVARCHAR (75)  DEFAULT ('') NOT NULL,
    [Soyad]                NVARCHAR (75)  DEFAULT ('') NOT NULL,
    PRIMARY KEY CLUSTERED ([MusteriID] ASC),
    CONSTRAINT [FK_Musteriler_Adresler] FOREIGN KEY ([AdresID]) REFERENCES [dbo].[Adresler] ([AdresID]),
    CONSTRAINT [FK_Musteriler_Kullanicilar] FOREIGN KEY ([OlusturanKullaniciID]) REFERENCES [dbo].[Kullanicilar] ([KullaniciID]),
    CONSTRAINT [CHK_MusteriTuru] CHECK ([MusteriTuru]='Kurumsal' OR [MusteriTuru]='Bireysel')
);

CREATE TABLE [dbo].[Siparisler] (
    [SiparisID]     INT           IDENTITY (1, 1) NOT NULL,
    [SiparisNo]     NVARCHAR (50) NOT NULL,
    [SiparisTarihi] DATETIME      DEFAULT (getdate()) NOT NULL,
    [KullaniciID]   INT           NOT NULL,
    [MusteriID]     INT           NOT NULL,
    [AdresID]       INT           NOT NULL,
    [SiparisDurumu] NVARCHAR (50) NOT NULL,
    [FaturalandiMi] BIT           DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([SiparisID] ASC),
    UNIQUE NONCLUSTERED ([SiparisNo] ASC),
    CONSTRAINT [FK_Siparisler_Kullanicilar] FOREIGN KEY ([KullaniciID]) REFERENCES [dbo].[Kullanicilar] ([KullaniciID]),
    CONSTRAINT [FK_Siparisler_Musteriler] FOREIGN KEY ([MusteriID]) REFERENCES [dbo].[Musteriler] ([MusteriID]),
    CONSTRAINT [FK_Siparisler_Adresler] FOREIGN KEY ([AdresID]) REFERENCES [dbo].[Adresler] ([AdresID]),
    CHECK ([SiparisDurumu]='Iptal' OR [SiparisDurumu]='Teslim Edildi' OR [SiparisDurumu]='Hazirlaniyor')
);

CREATE TABLE [dbo].[SiparisKalemleri] (
    [SiparisKalemID] INT             IDENTITY (1, 1) NOT NULL,
    [SiparisID]      INT             NOT NULL,
    [UrunID]         INT             NOT NULL,
    [Adet]           INT             NOT NULL,
    [BirimFiyat]     DECIMAL (18, 2) NOT NULL,
    [IptalEdildi]    BIT             DEFAULT ((0)) NOT NULL,
    [DepoID]         INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([SiparisKalemID] ASC),
    CONSTRAINT [FK_SiparisKalemleri_Depolar] FOREIGN KEY ([DepoID]) REFERENCES [dbo].[Depolar] ([DepoID]),
    CONSTRAINT [FK_SiparisKalemleri_Siparisler] FOREIGN KEY ([SiparisID]) REFERENCES [dbo].[Siparisler] ([SiparisID]),
    CONSTRAINT [FK_SiparisKalemleri_Urunler] FOREIGN KEY ([UrunID]) REFERENCES [dbo].[Urunler] ([UrunID])
);

CREATE TABLE [dbo].[Faturalar] (
    [FaturaID]     INT             IDENTITY (1, 1) NOT NULL,
    [FaturaNo]     NVARCHAR (50)   NOT NULL,
    [FaturaTarihi] DATETIME        DEFAULT (getdate()) NOT NULL,
    [SiparisID]    INT             NOT NULL,
    [ToplamTutar]  DECIMAL (18, 2) NOT NULL,
    [OdemeDurumu]  NVARCHAR (50)   NOT NULL,
    [KullaniciID]  INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([FaturaID] ASC),
    UNIQUE NONCLUSTERED ([FaturaNo] ASC),
    CONSTRAINT [FK_Faturalar_Siparisler] FOREIGN KEY ([SiparisID]) REFERENCES [dbo].[Siparisler] ([SiparisID]),
    CONSTRAINT [FK_Faturalar_Kullanicilar] FOREIGN KEY ([KullaniciID]) REFERENCES [dbo].[Kullanicilar] ([KullaniciID]),
    CHECK ([OdemeDurumu]='Iptal Edildi' OR [OdemeDurumu]='Bekliyor' OR [OdemeDurumu]='Ödendi')
);

CREATE TABLE [dbo].[DepoStok] (
    [DepoStokID] INT IDENTITY (1, 1) NOT NULL,
    [DepoID]     INT NOT NULL,
    [UrunID]     INT NOT NULL,
    [Miktar]     INT DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([DepoStokID] ASC),
    CONSTRAINT [UQ_Depo_Urun] UNIQUE NONCLUSTERED ([DepoID] ASC, [UrunID] ASC),
    CONSTRAINT [FK_DepoStok_Depolar] FOREIGN KEY ([DepoID]) REFERENCES [dbo].[Depolar] ([DepoID]),
    CONSTRAINT [FK_DepoStok_Urunler] FOREIGN KEY ([UrunID]) REFERENCES [dbo].[Urunler] ([UrunID])
);

CREATE TABLE [dbo].[StokHareketleri] (
    [StokHareketID]  INT             IDENTITY (1, 1) NOT NULL,
    [UrunID]         INT             NOT NULL,
    [DepoID]         INT             NOT NULL,
    [KullaniciID]    INT             NOT NULL,
    [SiparisKalemID] INT             NULL,
    [Miktar]         INT             NOT NULL,
    [BirimFiyat]     DECIMAL (18, 2) NOT NULL,
    [HareketTipi]    TINYINT         NOT NULL,
    [HareketTarihi]  DATETIME        DEFAULT (getdate()) NOT NULL,
    [IptalKaynakli]  BIT             DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([StokHareketID] ASC),
    CONSTRAINT [FK_StokHareketleri_Urunler] FOREIGN KEY ([UrunID]) REFERENCES [dbo].[Urunler] ([UrunID]),
    CONSTRAINT [FK_StokHareketleri_Depolar] FOREIGN KEY ([DepoID]) REFERENCES [dbo].[Depolar] ([DepoID]),
    CONSTRAINT [FK_StokHareketleri_Kullanicilar] FOREIGN KEY ([KullaniciID]) REFERENCES [dbo].[Kullanicilar] ([KullaniciID]),
    CONSTRAINT [FK_StokHareketleri_SiparisKalemleri] FOREIGN KEY ([SiparisKalemID]) REFERENCES [dbo].[SiparisKalemleri] ([SiparisKalemID]),
    CONSTRAINT [CK_StokHareketleri_HareketTipi] CHECK ([HareketTipi]=(2) OR [HareketTipi]=(1))
);

CREATE TABLE [dbo].[KullaniciGirisCikisLog] (
    [ID]          INT      IDENTITY (1, 1) NOT NULL,
    [KullaniciID] INT      NOT NULL,
    [GirisZamani] DATETIME DEFAULT (getdate()) NOT NULL,
    [CikisZamani] DATETIME NULL,
    [SonPing]     DATETIME NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Log_Kullanici] FOREIGN KEY ([KullaniciID]) REFERENCES [dbo].[Kullanicilar] ([KullaniciID])
);



  