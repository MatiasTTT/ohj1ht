using System;
using System.Collections.Generic;
using System.Xml;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace MatikanMaisteri;

/// @author Matias Turpeinen
/// @version 02.10.2025
/// <summary>
/// Ohjelma on peli jossa pelaaja kokeilee ratkoa matemaattisia kysymyksiä.
/// </summary>
public class MatikanMaisteri : PhysicsGame
{
    private PhysicsObject[] _tiiliRivi; // Tiilirivi edustaa edistymistä.
    private (string kysymys, int vastaus) _nykyinenLasku; // Säilyttää sen hetkisen laskun sekä ennalta määritetyn vastauksen.
    private InputBox _pelaajanVastausLaatikko; // Ottaa vastaan pelaajan syöttämän vastauksen.
    private int _nykyinenTiili = 0; // Alustettu täällä kun muuten arvo nollaantuu eikä eteneminen toimi.

    /// <summary>
    /// Käynnistää pelin ja luo taustakuvat, sekä kutsuu aliohjelmat.
    /// </summary>
    public override void Begin()
    {
        Level.Background.Color = Color.White;
        Camera.ZoomToLevel();
        
        TiilikentanLuoja();
        AsetaOhjaimet();
        VastausLaatikonLuoja();
        KysymysLaatikonLuoja();
    }
    
    
    /// <summary>
    /// Aliohjelma luo tiilirivin näkyviin peli-ikkunaan.
    /// </summary>
    public void TiilikentanLuoja()
    {
        // Luodaan taulukko jossa 10 alkiota edustamaan tiilejä.
        const int TIILIEN_MAARA = 10;
        _tiiliRivi = new PhysicsObject[TIILIEN_MAARA];
        // käydään taulukon alkiot läpi ja lisätään niihin tiilet.
        // Lisäksi tiilet asetetaan sopiville kohdille niin, että muodostuu rivi.
        for (int i = 0; i < _tiiliRivi.Length; i++)
        {
            const double TIILEN_LEVEYS = 60.0;
            const double TIILEN_KORKEUS = 60.0;
            PhysicsObject tiili = new PhysicsObject(TIILEN_LEVEYS, TIILEN_KORKEUS); // Muuttumattoman kokoinen tiili.
            tiili.Shape = Shape.Rectangle;
            tiili.Color = Color.LightBlue;

            const double TIILIEN_ALKU_X = 120.0;
            const double TIILIEN_VALI = 65.0;
            const double TIILIEN_KORKEUS = 200.0;
            tiili.Position = new Vector(Level.Left + TIILIEN_ALKU_X + i * TIILIEN_VALI, Level.Bottom + TIILIEN_KORKEUS);
            Add(tiili);
                
            // Taulukossa paikkan [i] alkio edustaa rivistössä esiintyvää vastaavaa tiiltä.
            _tiiliRivi[i] = tiili;
        }
    }
    
    
    /// <summary>
    /// Luo peliin laatikon jossa esitetään arvottu matemaattinen kysymys.
    /// </summary>
    public void KysymysLaatikonLuoja()
    {
        LaskunArpoja();
        
        Label kysymysNaytto = new Label(490,70, _nykyinenLasku.kysymys);
        kysymysNaytto.Font = new Font(60);
        kysymysNaytto.Position = new Vector(Level.Left + 350, Level.Bottom + 285);
        kysymysNaytto.Color = Color.LightYellow;
        kysymysNaytto.TextColor = Color.Black;
        
        Add(kysymysNaytto);
    }
    
    
    /// <summary>
    /// Luo tekstikentän johon pelaaja voi syöttää vastauksensa kysymykseen.
    /// </summary>
    public void VastausLaatikonLuoja()
    {
        _pelaajanVastausLaatikko = new InputBox(14);
        _pelaajanVastausLaatikko.Color = Color.Transparent;
        _pelaajanVastausLaatikko.Font = new Font(60);
        _pelaajanVastausLaatikko.MaxCharacters = 4;
        _pelaajanVastausLaatikko.Position = new Vector(Level.Left + 700, Level.Bottom + 285);
        
        Add(_pelaajanVastausLaatikko);
    }
   
    
    /// <summary>
    /// Tallentaa pelaajan vastauksen ja tarkistaa onko se oikein vai väärin.
    /// </summary>
    public void VastauksenTallenninJaTarkistin()
    {
        string syotettyVastaus = _pelaajanVastausLaatikko.Text;
        string oikeaVastaus = Muuntaja(_nykyinenLasku.vastaus);
        
        if (syotettyVastaus == oikeaVastaus)
        {
            if (_nykyinenTiili < _tiiliRivi.Length)
            {
                _tiiliRivi[_nykyinenTiili].Color = Color.LightGreen;
                _nykyinenTiili++;
            }
            if (_nykyinenTiili >= _tiiliRivi.Length)
            {
                MessageDisplay.Add("MAHTAVAA! Kaikki oikein! VOITIT PELIN!!");
                MessageDisplay.BackgroundColor = Color.Transparent;
                MessageDisplay.Position = new Vector(0, Level.Bottom + 600);
                _pelaajanVastausLaatikko.Destroy();
                ConfirmExit();
                return;
            }
            _pelaajanVastausLaatikko.Destroy();
            VastausLaatikonLuoja();
            MessageDisplay.BackgroundColor = Color.Transparent;
            MessageDisplay.Add($"Hienoa vastaus {syotettyVastaus} oli oikein!");
            MessageDisplay.Font = new Font(60);
            MessageDisplay.TextColor = Color.Black;
            MessageDisplay.Position = new Vector(0, Level.Bottom + 450);
            KysymysLaatikonLuoja();
        }
        else
        {
            _pelaajanVastausLaatikko.Destroy();
            VastausLaatikonLuoja();
            MessageDisplay.BackgroundColor = Color.Transparent;
            MessageDisplay.Add($"Vastaus {syotettyVastaus} oli väärin, kokeile uudelleen.");
            MessageDisplay.Font = new Font(60);
            MessageDisplay.TextColor = Color.Black;
            MessageDisplay.Position = new Vector(0, Level.Bottom + 450);
            KysymysLaatikonLuoja();
        }
    }
    
    
    /// <summary>
    /// Muuttaa puretun tuple parin vastauksen int tyypistä stringiksi myöhempää vertailua varten.
    /// </summary>
    /// <param name="saatuNykyinenLaskuVastaus"></param>
    /// <returns>Palauttaa tyyppimuunnetun luvun.</returns>
    public string Muuntaja(int saatuNykyinenLaskuVastaus)
    {
        string vastauksenTyyppiMuunnos = saatuNykyinenLaskuVastaus.ToString();
        return vastauksenTyyppiMuunnos;
    }
   
    
    /// <summary>
    /// Arpoo listasta jonkin laskun, sekä sen vastauksen pelaajalle ratkaistavaksi.
    /// </summary>
    public void LaskunArpoja()
    {
        (string kysymys, int vastaus)[] laskut =
        {
            ("7+5", 12), ("9-3", 6), ("3*4", 12),
            ("12/3", 4), ("6+2+5", 13), ("10-4+2", 8),
            ("2*5+3", 13), ("14-7", 7), ("8+6-5", 9),
            ("(9+3)/4", 3), ("3*(5-2)", 9), ("(8+4)/2", 6),
            ("7+(12/3)", 11), ("(6*2)-5", 7), ("15-(9-4)", 10),
            ("(4*3)+2", 14), ("(14/2)+1", 8), ("5*(4-3)+7", 12),
            ("(9+6)/3", 5), ("(5*5)-10", 15), ("(12-5)*2+1", 15),
            ("(18/3)+(2*4)", 14), ("(6*3)-(8/2)", 14), ("(5+7)-(9/3)", 9),
            ("4*(9-7)+3", 11), ("(15-6)/3+8", 11), ("3*(8-5)+(12/4)", 12),
            ("(9-(6/3))*2", 14), ("((5*4)-(12/3))/2", 8), ("((3+9)/3)+(7-5)", 6),
            ("8*2-7", 9), ("(16/4)+9-5", 8), ("7+(8*2)-13", 10),
            ("(20-6)/2", 7), ("3*(7-3)+5", 17), ("(18/2)-(4*2)", 1),
            ("(10+14)/4", 6), ("6*(3-1)+4", 16), ("(9+9)/3+2", 8),
            ("5*3-(12/4)", 12), ("(8*2)+(15/5)-7", 12), ("14-(3*(8-6))", 8),
            ("(27/3)-5+4", 8), ("(4+6)*(3-2)", 10), ("7*(5-3)-6", 8),
            ("(21/7)+(6*2)-5", 10), ("3+(18/6)+7", 13), ("(5*6)-(7+8)", 15),
            ("((8+10)/2)-3", 6), ("(4*(9-7))+(16/4)", 12), ("(15+9-6)/3", 6),
            ("(5*5)+(12/3)-8", 21), ("22-(6+4*3)", 4), ("3*(9/3)+(7-2)", 14),
            ("(12-(8/2))*2", 16), ("(28/4)+(3*(6-4))", 13), ("18-(3*(10-7))", 9),
            ("(6*4)-(20/5)", 20), ("4*(7-5)+(18/6)", 11)
        };
        int i = RandomGen.NextInt(laskut.Length);
        _nykyinenLasku = laskut[i];
    }
   
    
    /// <summary>
    /// Asetetaan näppäimet mistä käyttäjän vastaus hyväksytään ja millä peli suljetaan.
    /// </summary>
   public void AsetaOhjaimet()
    {
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Enter, ButtonState.Pressed, VastauksenTallenninJaTarkistin, null);
    }
    
    
}