using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Widgets;

/// <summary>
/// 
/// </summary>

/// @author Elisa Autonen
/// @version 8.12.2019
/// <summary>
/// Karkkimaa on peli, jossa prinsessa kerää karkkeja ja varoo osumasta peikkoihin
/// </summary>
public class Karkkimaa : PhysicsGame
{
    private const int PEIKONKORKEUS = 100;
    private const int PEIKONLEVEYS = 100;
    private const int PRINSESSANLEVEYS = 50;
    private const int PRINSESSANKORKEUS = 150;
    private const int KARKKIENMAARA = 3;
    private const int PEIKKOJENMAARA = 2;
    private const int PRINSESSAPAIKKA = 0;
    private const int KARKKIPAIKKA = 100;

    private Vector nopeusYlos = new Vector(0, 500);
    private Vector nopeusAlas = new Vector(0, -500);
    private Vector nopeusVasen = new Vector(-500, 0);
    private Vector nopeusOikea = new Vector(500, 0);

    
    int edellinenArvo = 0;

    IntMeter pisteLaskuri;
    IntMeter elamaLaskuri;
    /// <summary>
    /// Luodaan peliin alkuvalikko, josta aloitetaan peli
    /// </summary>
    public override void Begin()
    {
        Level.Background.Color = Color.Black;
        MultiSelectWindow valikko = new MultiSelectWindow("KARKKIMAA",
        "Aloita peli", "Lopeta");
        valikko.ItemSelected += Peliloppui;
        valikko.Color = Color.Pink;
        Add(valikko);
    }
    /// <summary>
    /// Luodaan kenttä. Kutsutaan aliohjelmia, joissa luodaan hahmot ja pelin toiminnot
    /// </summary>
    private void LuoKentta()
    {
        Camera.ZoomToLevel();
        Level.Background.CreateGradient(Color.Black, Color.LightPink);

        Level.Size = Screen.Size;
        // Level.Width = Screen.Width
        // Level.Height = Screen.Height;
        Camera.ZoomToLevel();
        

        PhysicsObject prinsessa = LuoPrinsessa(this);
        Add(prinsessa);
        AddCollisionHandler(prinsessa, "peikko", PelaajaOsuu);
        AddCollisionHandler(prinsessa, "karkki", KeraaKarkki);
        AsetaOhjaimet(prinsessa);
        LuoKarkki();  
        LuoPeikko();
        LuoPistelaskuri();
        LuoElamaLaskuri(prinsessa);
        KarkkiAjastin();
        PeikkoAjastin();
    }
    /// <summary>
    /// Luodaan pelaaja eli prinsessa 
    /// </summary>
    /// <param name="Karkkimaa">peli</param>
    /// <returns></returns>
    private PhysicsObject LuoPrinsessa(PhysicsGame Karkkimaa)
    {
   
        PhysicsObject prinsessa = Olio(this, PRINSESSANLEVEYS, PRINSESSANKORKEUS, PRINSESSAPAIKKA, PRINSESSAPAIKKA,
        "prinsessa", false, false, false, 0, "prinsessa", Color.Transparent, Shape.Rectangle, false);
        return prinsessa;
    }


    /// <summary>
    /// Luodaan peikkoja kentälle satunnaisiin paikkoihin
    /// </summary>
    private void LuoPeikko()
    {
        for(int i = 0; i < PEIKKOJENMAARA; i++)
        { 
            double x = RandomGen.NextDouble(Level.Left, Level.Right);
            double y = RandomGen.NextDouble(Level.Bottom, Level.Top);
            PhysicsObject peikko = Olio(this, PEIKONLEVEYS, PEIKONKORKEUS, x, y,
            "peikko", true, true, false, 0, "peikko", Color.Transparent, Shape.Rectangle, true);
            Add(peikko);

            //Tehdään ´peikolle polkuaivot
            PathFollowerBrain peikonAivot = new PathFollowerBrain(200);

            peikko.Brain = peikonAivot;
            List<Vector> polku = new List<Vector>();
            polku.Add(Vector.FromLengthAndAngle(RandomGen.NextDouble(Screen.Top * 2, Screen.Top * 3), RandomGen.NextAngle()));
            peikonAivot.Path = polku;
            peikonAivot.Loop = true;
            peikonAivot.Speed = 250;
        }
    }


    /// <summary>
    /// Tehdään taulukko erilaisille karkeille ja luodaan karkit karkkitaulukosta
    /// </summary>
    private void LuoKarkki()
    {
        string[] karkkitaulukko = { "karkki", "kuva2", "karkki3"};
        for (int i = 0; i < KARKKIENMAARA; i++)
        {
            PhysicsObject karkki = Olio(this, KARKKIPAIKKA, KARKKIPAIKKA, RandomGen.NextDouble(Level.Left, Level.Right), RandomGen.NextDouble(Level.Bottom, Level.Top), "karkki", false, false, true, 10, RandomGen.SelectOne(karkkitaulukko), RandomGen.NextColor(), Shape.Circle, true);
            Add(karkki);
        }
    }

    /// <summary>
    /// Luodaan oliot.
    /// </summary>
    /// <param name="peli"></param>
    /// <param name="leveys">olion leveys</param>
    /// <param name="korkeus">olion korkeus</param>
    /// <param name="x">olion paikka x-akselin suhteen</param>
    /// <param name="y">olion paikka y-akselin suhteen</param>
    /// <param name="tag">tagi</param>
    /// <param name="painovoima">vaikuttaako painovoima</param>
    /// <param name="fysiikanlait">vaikuttaako fysiiikan lait</param>
    /// <param name="tormayksethuomiotta">onko läpimentävä</param>
    /// <param name="lepokitkaArvo">Lepokitka</param>
    /// <param name="elinaika">elinaika lisäämisen jälkeen</param>
    /// <param name="kuva">kuva</param>
    /// <param name="vari">vari</param>
    /// <param name="muoto">muoto</param>
    /// <param name="kaantyyko">pyöriikö olio vapaasti</param>
    /// <returns></returns>
   
    private PhysicsObject Olio(PhysicsGame peli, int leveys, int korkeus, double x, double y, string tag, Boolean painovoima, Boolean fysiikanlait, Boolean tormayksethuomiotta, double lepokitkaArvo, String kuva, Color vari, Shape muoto, Boolean kaantyyko)
    {
        PhysicsObject olio = new PhysicsObject(leveys, korkeus, Shape.Rectangle);
        olio.X = x;
        olio.Y = y;
        olio.Tag = tag;
        olio.IgnoresGravity = painovoima;
        olio.IgnoresPhysicsLogics = fysiikanlait;
        olio.IgnoresCollisionResponse = tormayksethuomiotta;
        olio.StaticFriction = lepokitkaArvo;  
        olio.Image = LoadImage(kuva);
        olio.Color = vari;
        olio.Shape = muoto;
        olio.CanRotate = kaantyyko;
        if(tag == "peikko") olio.LifetimeLeft = TimeSpan.FromSeconds(6);
        return olio;
    }


    /// <summary>
    /// Luodaan Ajastin, joka käskee luoda karkkeja tietyin väliajoin
    /// </summary>
   
    private void KarkkiAjastin()
    {
        Timer LisaaKarkkia = new Timer();
        LisaaKarkkia.Start();
        LisaaKarkkia.Interval = 5.0;
        LisaaKarkkia.Timeout += LuoKarkki;
        
    }


    /// <summary>
    /// Luodaan ajastin, joka käskee luoda peikkoja tietyin väliajoin
    /// </summary>
    
    private void PeikkoAjastin()
    {
        Timer synnytaPeikkoja = new Timer();
        synnytaPeikkoja.Interval = 2.0;
        synnytaPeikkoja.Timeout += LuoPeikko;
        synnytaPeikkoja.Start();
    }


    /// <summary>
    /// Luodaan elämälaskuri
    /// </summary>
    /// <param name="prinsessa">prinsessa</param>
    /// <param name="peikko">peikko</param>
    void LuoElamaLaskuri(PhysicsObject prinsessa)
    {
        elamaLaskuri = new IntMeter(3);

        Label elamaNaytto = new Label();
        elamaNaytto.X = Screen.Right -50;
        elamaNaytto.Y = Screen.Top - 50;
        elamaNaytto.TextColor = Color.White;
        elamaNaytto.Color = Color.Purple;

        elamaNaytto.BindTo(elamaLaskuri);
        Add(elamaNaytto);
        IntMeter laskuri = new IntMeter(1);
    }

    /// <summary>
    /// Jos pelaaja osuu peikkoon, vähennetään elämiä ja kun niitä on liian vähän, hävitään peli ja siirrytään valikkoon,
    /// josta voi joko aloittaa uuden pelin tai poistua pelistä.
    /// </summary>
    /// <param name="prinsessa"></param>
    /// <param name="peikko"></param>
    private void PelaajaOsuu(PhysicsObject prinsessa, PhysicsObject peikko)
    {
        Vector ylos = new Vector(200, 0);
        AddCollisionHandler(peikko, "trampoliini", CollisionHandler.HitTarget(ylos));
        elamaLaskuri.Value -= 1;

        if (elamaLaskuri.Value == 0)
        { 
            Explosion rajahdys = new Explosion(500);
            rajahdys.Position = prinsessa.Position;
            rajahdys.Speed = 100.0;
            rajahdys.Force = 100000;
            rajahdys.Image = LoadImage("rajahdys");
            rajahdys.ShockwaveColor = new Color(255, 0, 150, 100);
            Add(rajahdys);
            MultiSelectWindow valikko = new MultiSelectWindow("Peli loppui :(",
            "Uusi peli", "Lopeta");
            valikko.ItemSelected += Peliloppui;
            valikko.Color = Color.Violet;
            Add(valikko);
        }

    }

    /// <summary>
    /// Tehdään loppuvalikko, josta voi joko aloittaa uuden pelin tai poistua
    /// </summary>
    /// <param name="valinta"></param>
     

    private void Peliloppui(int valinta)
    {
        switch (valinta)
        {
            case 0:
                ClearAll();
                LuoKentta();
                break;
            case 1:
                Exit();
                break;
        }
    }

    /// <summary>
    /// Luodaan pistelaskuri karkeille.
    /// </summary>
    private void LuoPistelaskuri()
    {
        pisteLaskuri = new IntMeter(0);

        Label pisteNaytto = new Label();
        pisteNaytto.X = 0;
        pisteNaytto.Y = Screen.Top - 50;
        pisteNaytto.TextColor = Color.White;
        pisteNaytto.Color = Color.LightPink;
        pisteNaytto.BindTo(pisteLaskuri);
        Add(pisteNaytto);
        IntMeter laskuri = new IntMeter(1);
    }

    /// <summary>
    /// Määrätään mitä tapahtuu kun törmätään karkkiin.
    /// Prinsessa syö karkin ja pisteet kasvavat
    /// </summary>
    /// <param name="prinsessa"></param>
    /// <param name="karkki"></param>
    private void KeraaKarkki(PhysicsObject prinsessa, PhysicsObject karkki)
    {
        karkki.Destroy();
        pisteLaskuri.Value += 1;
        if (pisteLaskuri.Value - edellinenArvo == 10)
        {
            elamaLaskuri.Value += 1;
            edellinenArvo = pisteLaskuri.Value;
        }
    }

    /// <summary>
    /// Asetetaan pelille ohjaimet
    /// </summary>
    /// <param name="prinsessa">prinsessa</param>
    private void AsetaOhjaimet(PhysicsObject prinsessa)
    {
        Keyboard.Listen(Key.Up, ButtonState.Down, AsetaNopeus, "prinsessa: Liiku ylös", prinsessa, nopeusYlos);
        Keyboard.Listen(Key.Up, ButtonState.Released, AsetaNopeus, null, prinsessa, Vector.Zero);
        Keyboard.Listen(Key.Down, ButtonState.Down, AsetaNopeus, "prinsessa: Liiku alas", prinsessa, nopeusAlas);
        Keyboard.Listen(Key.Down, ButtonState.Released, AsetaNopeus, null, prinsessa, Vector.Zero);

        Keyboard.Listen(Key.Right, ButtonState.Down, AsetaNopeus, "prinsessa: Liiku oikealle", prinsessa, nopeusOikea);
        Keyboard.Listen(Key.Right, ButtonState.Released, AsetaNopeus, null, prinsessa, Vector.Zero);

        Keyboard.Listen(Key.Left, ButtonState.Down, AsetaNopeus, "prinsessa: Liiku alas", prinsessa, nopeusVasen);
        Keyboard.Listen(Key.Left, ButtonState.Released, AsetaNopeus, null, prinsessa, Vector.Zero);

        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    /// <summary>
    /// Asetetaan nopeudet
    /// </summary>
    /// <param name="prinsessa"></param>
    /// <param name="nopeus">nopeus</param>
    private void AsetaNopeus(PhysicsObject prinsessa, Vector nopeus)
    {
        if ((nopeus.Y < 0) && (prinsessa.Bottom < Level.Bottom))
        {
            prinsessa.Velocity = Vector.Zero;
            return;
        }
        if ((nopeus.Y > 0) && (prinsessa.Top > Level.Top))
        {
            prinsessa.Velocity = Vector.Zero;
            return;
        }

        if ((nopeus.X > 0) && (prinsessa.Right > Level.Right))
        {
            prinsessa.Velocity = Vector.Zero;
            return;
        }

        if ((nopeus.X < 0) && (prinsessa.Left < Level.Left))
        {
            prinsessa.Velocity = Vector.Zero;
            return;
        }
        prinsessa.Velocity = nopeus;
    }
}
