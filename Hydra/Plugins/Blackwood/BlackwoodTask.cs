namespace StockSharp.Hydra.Blackwood
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Net;
	using System.Security;

	using Ecng.Common;
	using Ecng.Xaml;

	using StockSharp.Algo.Candles;
	using StockSharp.Blackwood;
	using StockSharp.BusinessEntities;
	using StockSharp.Hydra.Core;
	using StockSharp.Messages;

	using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

	using StockSharp.Localization;

	[Category(TaskCategories.American)]
	[TaskDisplayName(_sourceName)]
	class BlackwoodTask : ConnectorHydraTask<BlackwoodTrader>
	{
		private const string _sourceName = "Fusion/Blackwood";

		[TaskSettingsDisplayName(_sourceName)]
		private sealed class BlackwoodSettings : ConnectorHydraTaskSettings
		{
			public BlackwoodSettings(HydraTaskSettings settings)
				: base(settings)
			{
			}

			[TaskCategory(_sourceName)]
			[DisplayNameLoc(LocalizedStrings.Str1445Key)]
			[DescriptionLoc(LocalizedStrings.Str1445Key, true)]
			[PropertyOrder(0)]
			public string Login
			{
				get { return (string)ExtensionInfo["Login"]; }
				set { ExtensionInfo["Login"] = value; }
			}

			[TaskCategory(_sourceName)]
			[DisplayNameLoc(LocalizedStrings.Str1447Key)]
			[DescriptionLoc(LocalizedStrings.Str1448Key)]
			[PropertyOrder(1)]
			public SecureString Password
			{
				get { return ExtensionInfo["Password"].To<SecureString>(); }
				set { ExtensionInfo["Password"] = value; }
			}

			[TaskCategory(_sourceName)]
			[DisplayNameLoc(LocalizedStrings.Str3694Key)]
			[DescriptionLoc(LocalizedStrings.Str3695Key)]
			[PropertyOrder(2)]
			public EndPoint HistoricalDataAddress
			{
				get { return ExtensionInfo["HistoricalDataAddress"].To<EndPoint>(); }
				set { ExtensionInfo["HistoricalDataAddress"] = value.To<string>(); }
			}

			[TaskCategory(_sourceName)]
			[DisplayNameLoc(LocalizedStrings.Str3696Key)]
			[DescriptionLoc(LocalizedStrings.Str3697Key)]
			[PropertyOrder(3)]
			public EndPoint MarketDataAddress
			{
				get { return ExtensionInfo["MarketDataAddress"].To<EndPoint>(); }
				set { ExtensionInfo["MarketDataAddress"] = value.To<string>(); }
			}

			[Browsable(true)]
			public override bool IsDownloadNews
			{
				get { return base.IsDownloadNews; }
				set { base.IsDownloadNews = value; }
			}
		}

		public BlackwoodTask()
		{
			_supportedCandleSeries = BlackwoodSessionHolder.TimeFrames.Select(tf => new CandleSeries
			{
				CandleType = typeof(TimeFrameCandle),
				Arg = tf
			}).ToArray();
		}

		private BlackwoodSettings _settings;

		public override Uri Icon
		{
			get { return "bw_logo.png".GetResourceUrl(GetType()); }
		}

		public override string Description
		{
			get { return LocalizedStrings.Str2281Params.Put(_sourceName); }
		}

		public override HydraTaskSettings Settings
		{
			get { return _settings; }
		}

		private readonly Type[] _supportedMarketDataTypes = { /*typeof(MarketDepth),*/ typeof(Candle), typeof(Trade), typeof(Level1ChangeMessage) };

		public override IEnumerable<Type> SupportedMarketDataTypes
		{
			get { return _supportedMarketDataTypes; }
		}

		private readonly IEnumerable<CandleSeries> _supportedCandleSeries;

		public override IEnumerable<CandleSeries> SupportedCandleSeries
		{
			get { return _supportedCandleSeries; }
		}

		protected override MarketDataConnector<BlackwoodTrader> CreateTrader(HydraTaskSettings settings)
		{
			_settings = new BlackwoodSettings(settings);

			if (settings.IsDefault)
			{
				_settings.Login = string.Empty;
				_settings.Password = new SecureString();
				_settings.IsDownloadNews = true;
				_settings.SupportedLevel1Fields = Enumerator.GetValues<Level1Fields>();

				_settings.HistoricalDataAddress = new IPEndPoint(BlackwoodAddresses.WetBush, BlackwoodAddresses.HistoricalDataPort);
				_settings.MarketDataAddress = new IPEndPoint(BlackwoodAddresses.WetBush, BlackwoodAddresses.MarketDataPort);
			}

			return new MarketDataConnector<BlackwoodTrader>(EntityRegistry.Securities, this, () => new BlackwoodTrader
			{
				HistoricalDataAddress = _settings.HistoricalDataAddress,
				MarketDataAddress = _settings.MarketDataAddress,
				Login = _settings.Login,
				Password = _settings.Password.To<string>(),
			});
		}
	}
}