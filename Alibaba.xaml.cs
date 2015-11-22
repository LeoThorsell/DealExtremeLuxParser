using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DealExtremeComparison
{
	/// <summary>
	/// Interaction logic for Alibaba.xaml
	/// </summary>
	public partial class Alibaba : Window
	{
		public ObservableCollection<RowItem> Items { get; set; }
		public Alibaba()
		{
			Items = new ObservableCollection<RowItem>();
			this.DataContext = this;
			InitializeComponent();
		}
		private async Task ParseFromAddress(string url)
		{
			var response = await new System.Net.Http.HttpClient().GetStreamAsync(url);
			var page = new HtmlAgilityPack.HtmlDocument();
			page.Load(response);
			var itemNodes = page.DocumentNode.SelectNodes("//form[@id='list-form']/div[@class='list-items']/div[@class='percent-wrap']");
			foreach (var item in itemNodes)
			{
				var titleNode = item.SelectNodes(".//p[@class='title']/a").FirstOrDefault();
				var priceNode = item.SelectNodes(".//p[@class='price']").FirstOrDefault();
				string link = titleNode.Attributes["href"].Value;
				string title = titleNode.Attributes["title"].Value;
				double price;
				double.TryParse(priceNode.InnerText.Trim().Replace("US$", "").Replace(".",","), out price);
				var lumenMatch = new Regex(@"\d{3,4}[-\s]?(lm|lumen)", RegexOptions.IgnoreCase).Match(title);
				if (!lumenMatch.Groups[0].Success)
					continue;
				string lumenCountString = Regex.Replace(lumenMatch.Groups[0].Value, "[^0-9]", "");
				int lumenCount;
				if (!int.TryParse(lumenCountString, out lumenCount))
					continue;
				Items.Add(new RowItem
					{
						LumenCount =  lumenCount,
						Price = price,
						Link = "http://dx.com" + link
					});
			}
			this.Title = Items.Count.ToString();
		}
	}
}
