using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

namespace DeclarationOfConformity
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SQLiteConnection connection = new SQLiteConnection("Data Source=./data/DataSourceConfis.db");
        public string initialText = "--- bitte auswählen ---";
        public MainWindow()

        {
            InitializeComponent();
            FillVarianten();
            SprachenLesen();
            DatumDP.SelectedDate = DateTime.Today;
        }

        // Befüllen der Comboboxen
        public void FillVarianten()
        {
            
            string query = CreateQueryFzTyp();
            connection.Open();
            SQLiteCommand cmd = new SQLiteCommand(query, connection); // Verwendet angegebenes query zum Einschränken der Auswahl in der Combobox
            SQLiteDataReader reader = cmd.ExecuteReader();
            variantenCMB.Items.Add(initialText); // Setzt den initialen Text "--- bitte auswählen ---" in die Combobox
            try
            {
                while (reader.Read())
                {
                    string sVariante = reader.GetString(0); // Fügt die Werte des query-Ergebnisses in die Variable und anschließend in die Combobox
                    variantenCMB.Items.Add(sVariante);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                connection.Close();
            }
            variantenCMB.SelectedIndex = 0;
            
        }
        public void SprachenLesen()
        {
            string query = CreateQuerySprachen();
            connection.Open();
            SQLiteCommand cmd = new SQLiteCommand(query, connection);
            SQLiteDataReader reader = cmd.ExecuteReader();
            SprachenCMB.Items.Add(initialText);
            
            try
            {
                while (reader.Read())
                {
                    string sSprache = reader.GetString(0);
                    SprachenCMB.Items.Add(sSprache);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                connection.Close();
            }
            SprachenCMB.SelectedIndex = 0;

        }

        //Plausibilitätsprüfungen
        public void CheckSerialNumber()
        {
            int anzahlZeichen = SerialNumberTxt.Text.Length; // Ermittelt die Zeichenlänge der Serialnummer --> Wird auf Typenschild immer 17Stellig aufgeführt. Fehlende Stellen werden mit Nullen aufgefüllt vorangestellt.
            string valueTextBox = SerialNumberTxt.Text; // Speichert den Text falls < 17 Stellen eingegeben wurden.
            if (valueTextBox.Length > 17)
            { //Prüfung und Fehleingabebahndlung falls zu viele Zeichen eingegeben werden. Focus wird auf Textbox gesetzt, eine weiterführen ist ohne korrekte Eingabe nicht möglich.
                MessageBox.Show("Zu viele Zeichen eingegeben. Bitte Eingabge korrigieren!\nEs wurden " + anzahlZeichen + " eingegeben", "Fehler bei Eingabe", MessageBoxButton.OK, MessageBoxImage.Warning);
                SerialNumberTxt.Focus();
                return;
            }
            else if(valueTextBox.Length < 17)
            { // Weniger als 17 Stellen werden mit der Differenz aus 17 - der eingegebenen Zeichen ermittelt und aufgefüllt.
                string serialNumber = new StringBuilder().Insert(0, "0", 17 - anzahlZeichen).ToString();
                SerialNumberTxt.Text = serialNumber + valueTextBox; // der eingegebene Wert wird an die aufgefüllten Nullen angehängt, damit die Eingabe gültig ist.
            }
            else
            {
                CheckVariante();
            }
        } //Prüfung auf Richtigkeit Fahrgestellnummer
        public void CheckDatumsAuswahl()
        {
            string gewähltesDataum = DatumDP.Text;
            
            if (DateTime.Parse(gewähltesDataum) > DateTime.Today)
            {
                MessageBox.Show("Falsches Datum gewählt. Das Datum darf nicht in der Zukunft liegen!\nEs wurde der " + gewähltesDataum + " ausgewählt. Datum wurde auf tagesaktuelles Datum korrigiert.", "Fehler bei Eingabe", MessageBoxButton.OK, MessageBoxImage.Warning);
                DatumDP.SelectedDate = DateTime.Today;
                return;
            }
            else
            {
                CreateDataGrid();
            }
        } //Prüfung auf valides Datum
        public void CheckVariante()
        {
            if (variantenCMB.Text == initialText)
            {
                MessageBox.Show("Kein Fahrzeug ausgewählt. \nBitte wählen Sie ein Fahrzeug aus der Liste", "Fehler bei Eingabe", MessageBoxButton.OK, MessageBoxImage.Warning);
                variantenCMB.Focus();
                return;
            }
            else
            {
                CheckSprache();
            }
        }
        public void CheckSprache()
        {
            if (SprachenCMB.Text == initialText)
            {
                MessageBox.Show("Keine Sprache ausgewählt. \nBitte wählen Sie eine Sprache aus der Liste", "Fehler bei Eingabe", MessageBoxButton.OK, MessageBoxImage.Warning);
                SprachenCMB.Focus();
                return;
            }
            else
            {
                CheckDatumsAuswahl();
            }
        } //Prüfung ob eine Sprache ausgewählt wurde
        public void CheckDeclaration()
        {
            if (EG_Declaration.IsChecked !=true && Manufacturer_Declaration.IsChecked != true && UK_Declaration.IsChecked != true)
            {
                MessageBox.Show("Keine Art der Erklärung ausgewählt. \nEs wurde automatisch 'EG-Konformitätserklärung' gewählt\nBitte Auswahl prüfen und ggf. korrigieren", "Fehlende Eingabe", MessageBoxButton.OK, MessageBoxImage.Warning);
                EG_Declaration.IsChecked = true;
            }
            else
            {
                CheckVehicleType();
            }
            
        } //Prüfung welche Declaration gewählt wurde. Ggf. automatisches setzen.
        public void CheckVehicleType()
        {
            if (Radlader.IsChecked != true && Teleskoplader.IsChecked != true && ELader.IsChecked != true && Anbaugeräte.IsChecked != true)
            {
                MessageBox.Show("Keine Fahrzeugart ausgewählt. \nEs wurde automatisch 'Radlader' gewählt\nBitte Auswahl prüfen und ggf. korrigieren", "Fehlende Eingabe", MessageBoxButton.OK, MessageBoxImage.Warning);
                Radlader.IsChecked = true;
            }
            else
            {
                CheckSerialNumber();
            }
        }
        
        //Erstellen der Queryabfragen auf Grundlage der Auswahl in Radiobuttons
        public string CreateQueryFzTyp()
        {
            if (Radlader.IsChecked == true)
            {
                return "SELECT Maschinenvariante FROM tbl_variantenMaschinen WHERE Art = 'RL'";
            }
            else if (Teleskoplader.IsChecked == true)
            {
                return "SELECT Maschinenvariante FROM tbl_variantenMaschinen WHERE Art = 'TL'";
            }
            else if (ELader.IsChecked == true)
            {
                return "SELECT Maschinenvariante FROM tbl_variantenMaschinen WHERE Art = 'EL'";
            }
            else
            {
                return "SELECT Maschinenvariante FROM tbl_variantenMaschinen";
            }
        }
        public string CreateQuerySprachen()
        {
            if (EG_Declaration.IsChecked == true)
            {
                return "SELECT Sprache,EG FROM tbl_sprachen WHERE EG = 'x'";
            }
            else if (Manufacturer_Declaration.IsChecked == true)
            {
                return "SELECT Sprache, Hersteller FROM tbl_sprachen WHERE Hersteller = 'x'";
            }
            else
            {
                return "SELECT Sprache, UKCA FROM tbl_sprachen WHERE UKCA = 'x'";
            }

        }

        //Reload der Comboboxen, wenn sich die Auswahl der Radiobuttons ändert
        private void ReloadComboBox(object sender, RoutedEventArgs e)
        {
            
                variantenCMB.Items.Clear(); // Entfernt alle Einträge aus der Combobox und setzt je nach Auswahl die Elemente neu
            
            FillVarianten();
        }
        private void ReloadSprachen(object sender, RoutedEventArgs e)
        {
            SprachenCMB.Items.Clear();
            SprachenLesen();
        }

        //Erstellen des Datagrids zur Weiterverarbeitung
        public void CreateConformity(object sender, RoutedEventArgs e)
        {
            CheckDeclaration();
        }

        public void CreateDataGrid()
        {
            connection.Open();
            string tbl = "tbl_variantenMaschinen";
            string Fzg = variantenCMB.Text;
            if (Anbaugeräte.IsChecked==true)
            {
                tbl = "tbl_variantenAnbaugeräte";
            }
            string query = String.Format("SELECT * from {0} WHERE Maschinenvariante = '{1}'",tbl, Fzg);
            SQLiteCommand cmd = new SQLiteCommand(query,connection);
            SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(query, connection);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            dgErgebnis.ItemsSource = dt.AsDataView();
            for (int i = 3; i > -1; i--)
            {
                dgErgebnis.Columns.RemoveAt(i);
            }
            
            connection.Close();
            MessageBox.Show("Daten für die Konformitätserklärung wurden erzeugt. Bitte Tabelle prüfen und Konformitätserklärung drucken.", "Erstellung erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
            
        }

        private void CloseButton(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
    
