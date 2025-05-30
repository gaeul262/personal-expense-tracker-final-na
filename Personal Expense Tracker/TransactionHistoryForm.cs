using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace Personal_Expense_Tracker
{
    public partial class TransactionHistoryForm : Form
    {
        public TransactionHistoryForm()
        {
            InitializeComponent();
            cmbCategory.SelectedIndexChanged += cmbCategory_SelectedIndexChanged;
        }

        private void TransactionHistoryForm_Load(object sender, EventArgs e)
        {
            LoadCategories();
            LoadTransactions();
        }

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("All");
            cmbCategory.Items.Add("Food");
            cmbCategory.Items.Add("Transport");
            cmbCategory.Items.Add("Bills");
            cmbCategory.Items.Add("Snacks");

            cmbCategory.SelectedIndex = 0;
        }

        private void LoadTransactions(string filter = "")
        {
            DataTable table = new DataTable();
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("Category", typeof(string));
            table.Columns.Add("PaymentMethod", typeof(string));

            table.Rows.Add(500.00m, DateTime.Today, "Food", "Cash");
            table.Rows.Add(1200.00m, DateTime.Today.AddDays(-1), "Transport", "Credit Card");
            table.Rows.Add(800.00m, DateTime.Today.AddDays(-3), "Bills", "Online");
            table.Rows.Add(200.00m, DateTime.Today.AddDays(-5), "Snacks", "GCash");
            table.Rows.Add(150.00m, DateTime.Today.AddDays(-2), "Food", "Cash");

            var filteredRows = table.AsEnumerable();

            filteredRows = filteredRows.Where(row =>
                row.Field<DateTime>("Date") >= dtpFrom.Value.Date &&
                row.Field<DateTime>("Date") <= dtpTo.Value.Date);

            if (cmbCategory.SelectedItem != null && cmbCategory.SelectedItem.ToString() != "All")
            {
                string selectedCategory = cmbCategory.SelectedItem.ToString();
                filteredRows = filteredRows.Where(row =>
                    row.Field<string>("Category").Equals(selectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            string amountText = txtAmount.Text.Trim();
            if (!string.IsNullOrEmpty(amountText))
            {
                if (amountText.Contains("-"))
                {
                    var parts = amountText.Split('-');
                    if (parts.Length == 2 &&
                        decimal.TryParse(parts[0].Trim(), out decimal minAmount) &&
                        decimal.TryParse(parts[1].Trim(), out decimal maxAmount))
                    {
                        filteredRows = filteredRows.Where(row =>
                            row.Field<decimal>("Amount") >= minAmount &&
                            row.Field<decimal>("Amount") <= maxAmount);
                    }
                    else
                    {
                        MessageBox.Show("Invalid amount range format. Use min-max, e.g. 100-500.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    if (decimal.TryParse(amountText, out decimal amount))
                    {
                        filteredRows = filteredRows.Where(row =>
                            row.Field<decimal>("Amount") == amount);
                    }
                    else
                    {
                        MessageBox.Show("Invalid amount value.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }

            DataTable filteredTable;
            if (filteredRows.Any())
            {
                filteredTable = filteredRows.CopyToDataTable();
            }
            else
            {
                filteredTable = table.Clone();
            }

            dataGridView1.DataSource = filteredTable;

            UpdateSummary(filteredTable);
            UpdateChart(filteredTable);
        }

        private void UpdateSummary(DataTable table)
        {
            decimal total = 0;
            var summary = new StringBuilder();

            var groups = table.AsEnumerable()
                .GroupBy(row => row.Field<string>("Category"));

            foreach (var group in groups)
            {
                decimal categoryTotal = group.Sum(row => row.Field<decimal>("Amount"));
                total += categoryTotal;
                summary.AppendLine($"{group.Key}: ₱{categoryTotal:N2}");
            }

            lblTotal.Text = $"Total Spending: ₱{total:N2}";
            lblCategorySummary.Text = summary.Length > 0 ? summary.ToString() : "No data to summarize.";
        }

        private void UpdateChart(DataTable table)
        {
            chartSummary.Series.Clear();
            chartSummary.ChartAreas.Clear();

            var chartArea = new ChartArea();
            chartSummary.ChartAreas.Add(chartArea);

            var series = new Series
            {
                Name = "SpendingByCategory",
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true,
                LabelFormat = "₱{0:N2}"
            };

            chartSummary.Series.Add(series);

            var groups = table.AsEnumerable()
                .GroupBy(row => row.Field<string>("Category"));

            foreach (var group in groups)
            {
                decimal totalAmount = group.Sum(row => row.Field<decimal>("Amount"));
                series.Points.AddXY(group.Key, totalAmount);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
            Form expenseT = new ExpenseTracker();
            expenseT.Show();
        }

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        private void btnExportPDF_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Save transaction report",
                FileName = "TransactionReport.pdf"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Document doc = new Document(PageSize.A4, 10, 10, 10, 10);
                    PdfWriter.GetInstance(doc, new FileStream(saveFileDialog.FileName, FileMode.Create));
                    doc.Open();

                    Paragraph title = new Paragraph("Transaction History", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16));
                    title.Alignment = Element.ALIGN_CENTER;
                    doc.Add(title);
                    doc.Add(new Paragraph("\n"));

                    PdfPTable table = new PdfPTable(dataGridView1.Columns.Count);
                    table.WidthPercentage = 100;

                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        table.AddCell(new Phrase(column.HeaderText, FontFactory.GetFont(FontFactory.HELVETICA, 12)));
                    }

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                string cellText = cell.Value?.ToString() ?? "";
                                table.AddCell(new Phrase(cellText, FontFactory.GetFont(FontFactory.HELVETICA, 11)));
                            }
                        }
                    }

                    doc.Add(table);
                    doc.Close();

                    MessageBox.Show("PDF Exported Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting PDF: " + ex.Message);
                }
            }
        }

        private void TransactionHistoryForm_Load_1(object sender, EventArgs e)
        {

        }

        private void lblCategorySummary_Click(object sender, EventArgs e)
        {

        }
    }
}
