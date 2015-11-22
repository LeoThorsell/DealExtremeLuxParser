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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace DealExtremeComparison
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			this.DataContext = this; 
			Items = new ObservableCollection<RowItem>();
			
			InitializeComponent();
			Populate();			
		}
		private async void Populate()
		{
			//for (int i = 0; i < 15; i++ )
				//Populate("http://dx.com/c/home-garden-1099/lightings-1045/led-light-bulbs-1072/e27-1074?pageSize=200&page=" + i.ToString());
			//for( int i=1;i<5;i++ )
				//Populate("http://www.dx.com/c/flashlights-lasers-999/headlamps-906?pageSize=200&page=" + i.ToString());
			//for (int i = 1; i < 19; i++)
				//await Populate("http://www.dx.com/c/flashlights-lasers-999/led-flashlights-901?pageSize=200&page=" + i.ToString());
			for (int i = 1; i < 4; i++)
				await Populate("http://www.dx.com/c/flashlights-lasers-999/led-flashlights-901/aa-flashlights-931?pageSize=200&page=" + i.ToString());
			 
			//Populate("http://dx.com/c/home-garden-1099/lightings-1045/led-light-bulbs-1072/gu10-1076?pageSize=200&page=1");
			//Populate("http://dx.com/c/home-garden-1099/lightings-1045/led-light-bulbs-1072/gu10-1076?pageSize=200&page=2");
			//Populate("http://dx.com/c/home-garden-1099/lightings-1045/led-light-bulbs-1072/gu10-1076?pageSize=200&page=3");
			//Populate("http://dx.com/c/home-garden-1099/lightings-1045/led-light-bulbs-1072/gu10-1076?pageSize=200&page=4");
		}

		public  ObservableCollection<RowItem> Items { get; set; } 
		private async Task Populate(string url)
		{
			var response = await new System.Net.Http.HttpClient().GetStreamAsync(url);
			var page = new HtmlAgilityPack.HtmlDocument();
			page.Load(response);
			var itemNodes = page.DocumentNode.SelectNodes("//ul[@class='productList subList']/li");
			foreach (var item in itemNodes)
			{
				var titleNode = item.SelectNodes(".//p[@class='title']/a").FirstOrDefault();
				var priceNode = item.SelectNodes(".//p[@class='price']").FirstOrDefault();
				var reviewNode = item.SelectNodes(".//p[@class='review']/a").FirstOrDefault();
				string link = titleNode.Attributes["href"].Value;
				string title = titleNode.Attributes["title"].Value;
				double price;
				
				if (priceNode.ChildNodes.Count == 3)
					priceNode = priceNode.ChildNodes[2];

				double.TryParse(priceNode.InnerText.Trim().Replace("US$", "").Replace(".",","), out price);
				var lumenMatch = new Regex(@"\d{3,4}[-\s]?(lm|lumen)", RegexOptions.IgnoreCase).Match(title);
				if (!lumenMatch.Groups[0].Success)
					continue;
				string lumenCountString = Regex.Replace(lumenMatch.Groups[0].Value, "[^0-9]", "");
				int lumenCount;
				if (!int.TryParse(lumenCountString, out lumenCount))
					continue;
				int starCount = -1;
				int reviewCount = -1;
				if(reviewNode!=null)
				{
					var starsString = reviewNode.Attributes.Where(a => a.Name == "title").Select(a => a.Value).FirstOrDefault().Replace(" out of 5 starts","");
					var reviewString = reviewNode.SelectNodes(".//span").Where(a => a.Attributes.Count == 0).Select(a=>a.InnerText).FirstOrDefault();
					reviewString = reviewString.Replace("Reviews", "").Trim();
					int.TryParse(starsString, out starCount);
					int.TryParse(reviewString, out reviewCount);
				}
				Items.Add(new RowItem
					{
						LumenCount =  lumenCount,
						Price = price,
						Link = "http://dx.com" + link,
						Stars = starCount,
						ReviewCount = reviewCount
					});
			}
			this.Title = Items.Count.ToString();
		}
		
	}
	public class RowItem
	{
		public int LumenCount { get; set; }
		public double Price { get; set; }
		public string Link { get; set; }
		public int Stars { get; set; }
		public int ReviewCount { get; set; }
	}
}
