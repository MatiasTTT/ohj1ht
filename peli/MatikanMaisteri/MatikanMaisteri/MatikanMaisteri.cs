using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace MatikanMaisteri;

/// @author Matias Turpeinen
/// @version 02.10.2025
/// <summary>
/// Ohjelma on peli jossa pelaaja kokeilee ratkoa matemaattisia kysymyksiä aikaa vastaan.
/// </summary>
public class MatikanMaisteri : PhysicsGame
{

    PhysicsObject[,] tiiliRuudukko; //Tiiliruudukko edustaa "pelikenttää" johon pommit pudotetaan.
    DoubleMeter alaspainLaskuri;
    Timer aikalaskuri;
    Label aikaNaytto;
    
    public override void Begin()
    {
        // Luodaan taulukko jossa 10 riviä ja 15 saraketta.
        tiiliRuudukko = new PhysicsObject[10, 15];
        
        TiilikentanLuoja(tiiliRuudukko);
        KellonLuoja();
        AsetaOhjaimet();
        
    }

    /// <summary>
    /// Aliohjelma luo tiiliruudukko kentän näkyviin peli-ikkunaan.
    /// </summary>
    /// <param name="saatuTiiliRuudukko">Luotu moniulotteinen taulukko.</param>
    /// <returns>Täytetty kenttä(taulukko)</returns>
    public PhysicsObject[,] TiilikentanLuoja(PhysicsObject[,] saatuTiiliRuudukko)
    {
        // Asetetaan kentän taustaväri ja kamera.
        Level.Background.Color = Color.White;
        Camera.ZoomToLevel();
        
        // käydään moniulotteisen taulukon alkiot läpi ja lisätään niihin tiilet.
        // Lisäksi tiilet asetetaan sopiville kohdille niin, että muodostuu ruudukko.
        // Taulukossa i esittää riviä ja j esittää saraketta.
        for (int i = 0; i < saatuTiiliRuudukko.GetLength(0); i++)
        {
            for (int j = 0; j < saatuTiiliRuudukko.GetLength(1); j++)
            {
                PhysicsObject tiili = new PhysicsObject(40.0, 40.0);
                tiili.Shape = Shape.Rectangle;
                tiili.Color = Color.LightBlue;
                
                double x = Level.Left + 175 + j * 45;
                double y = Level.Bottom + 200 + i * 45;

                tiili.Position = new Vector(x, y);
                Add(tiili);
                
                // Taulukossa paikka [i, j] vastaa tiilen sijaintia ruudukossa.
                saatuTiiliRuudukko[i, j] = tiili;
                
                // OMA AJATUS: Tähän täytyy myös lisätä varmaan tuo kellotiilien lisäys jollain random logiikalla.
            }
        }
        return saatuTiiliRuudukko;
    }
    
    /// <summary>
    /// Luo kellon näytölle, jossa aikaa on aluksi 10 min (600 sekuntia).
    /// </summary>
    public void KellonLuoja()
    {
        alaspainLaskuri = new DoubleMeter(600);

        aikalaskuri = new Timer();
        aikalaskuri.Interval = 1.0; // Arvo jonka perusteella päivitetään kelloa, 1.0 = päivitys sekunnin välein.
        aikalaskuri.Timeout += KelloLaskeAlaspain; // Kutsutaana aliohjelmaa, muuttamaan lukua joka kerta kun tuo sekunnin intervalli on ohi.
        aikalaskuri.Start();
        
        aikaNaytto = new Label();
        aikaNaytto.Y = 250;
        aikaNaytto.Color = Color.White;
        aikaNaytto.TextColor = Color.Black;
        aikaNaytto.DecimalPlaces = 0;
        aikaNaytto.BindTo(alaspainLaskuri);
        Add(aikaNaytto);
    }

    /// <summary>
    /// Aliohjelma joka vähentää alaspainLaskurin laskurista arvoa aina kutsuttaessa yhden verran,
    /// jolloin kello menee alaspäin. Täällä muotoillaan myös pelkkä sekunti esitys minuuteiksi
    /// ja sekunneiksi.
    /// </summary>
    public void KelloLaskeAlaspain()
    {
        alaspainLaskuri.Value -= 1;

        if (alaspainLaskuri.Value <= 0)
        {
            MessageDisplay.Add("Peliaika loppui. Hävisit pelin!");
            aikalaskuri.Stop();
            // OMA AJATUS: Tänne täytyy laittaa sitten jotain aliohjelma kutsuja, että peli saadaan päättymään.
            // Mekanismi on todennäköisesti sama kuin pelin voitto tilanteessa, eli tuo pelin
            // päättymis mekanismi tulee toteuttaa omassa aliohjelmasssaan.
        }
        
        // Lasketaan minuutit ja sekunnit
        int kokonaistSek = (int)Math.Max(0, alaspainLaskuri.Value);
        int minuutit = kokonaistSek / 60; // Kertoo, montako täyttä minuuttia laskurin ajassa on.
        int sekunnit = kokonaistSek % 60; // Kertoo paljonko jää yli, kun kokonaissekunnit jaetaan.

        // Muotoillaan 2-numeroinen sekunti (esim. 01, 09)
        aikaNaytto.Text = minuutit.ToString("00") + ":" + sekunnit.ToString("00");
    }

    public void KysymysLaatiKonLuoja()
    {
        // kutsutaan laskun arpojaa
    }

    public void VastausLaatikonLuoja()
    {
        //Tänne syötetään luku. Täytyy tarkistaa että luku on oikeasti int eikä jotain muuta,
        // jos ei int niin pitää syöttää uudestaan.
        //kutsutaan laskun arpojaa, sekä vastauksen tarkistinta.
    }

    public void LaskunArpoja()
    {
        // Taulukko laskujen teksteille
        string[] laskut = {"7+5", "9-3", "3*4", "12/3", "6+2+5", "10-4+2", "2*5+3", "14-7", "8+6-5", "(9+3)/4", 
            "3*(5-2)", "(8+4)/2", "7+(12/3)", "(6*2)-5", "15-(9-4)", "(4*3)+2", "(14/2)+1", "5*(4-3)+7", "(9+6)/3",
            "(5*5)-10", "(12-5)*2+1", "(18/3)+(2*4)", "(6*3)-(8/2)", "(5+7)-(9/3)", "4*(9-7)+3", "(15-6)/3+8",
            "3*(8-5)+(12/4)", "(9-(6/3))*2", "((5*4)-(12/3))/2", "((3+9)/3)+(7-5)"};

        // Taulukko oikeille vastauksille samoissa indekseissä
        int[] vastaukset = {12, 6, 12, 4, 13, 8, 13, 7, 9, 3, 9, 6, 11, 7, 10, 14, 8, 12, 5, 15, 15, 14, 14, 9, 11, 11, 12, 14, 8, 6};
        
        int i = RandomGen.NextInt(laskut.Length);
        string lasku = laskut[i];
        int oikeaVastaus = vastaukset[i];
        
        
        // Jos tämä laitetaankin niin että nuo kysymys ja vastauslaatikon luojat
        // ottavat vastaan aina tuon i arvon eli niitä kutsuttaisiin täältä.
    }

    public void VastauksenTarkistin()
    {
        // jos vastaus on oikein niin kutsutaan pommin luojaa.
        // Lisäksi täytyy olla looppi joka arpoo taas uuden luvun.
    }

    public int PomminLuoja(int teho)
    {
        // Tämä pitää varmaan sitten myöhemmin yhdistää pudotusalueen logiikkaan.
        // Logiikka joka laskee monestikko tiilet on räjäytetty tulle sisältää sitten pelin voitto/lopetus logiikka.
    }
    
    

   public void AsetaOhjaimet()
    {
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        // Vasemmalle nuoli liikuttaa pudotusaluetta vasempaan yhden tiilin verran.
        // Oikealle nuoli liikuttaa pudotusaluetta oikeaan yhden tiilin verran.
        // Välilyönti pudottaa pommin.
        // Enter hyväksyy käyttäjän laskun vastauksen, jos tämä ei käy jo automaattisesti Console.ReadLinessä.
    }
}