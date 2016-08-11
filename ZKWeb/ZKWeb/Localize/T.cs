﻿using DotLiquid;

namespace ZKWeb.Localize {
	/// <summary>
	/// A class wrap original text and return translated text
	/// Translation will perform when calling ToString
	/// This class can convert to string implicit, or use ToString explicit
	/// </summary>
	public struct T : ILiquidizable {
		/// <summary>
		/// Original text
		/// </summary>
		private string Text { get; set; }

		/// <summary>
		/// Get the translation of text
		/// </summary>
		/// <param name="text">Original text</param>
		public T(string text) {
			Text = text;
		}

		/// <summary>
		/// Get translated text
		/// </summary>
		/// <param name="t">This object</param>
		public static implicit operator string(T t) {
			return t.ToString();
		}

		/// <summary>
		/// Support render to template
		/// </summary>
		/// <returns></returns>
		object ILiquidizable.ToLiquid() {
			return ToString();
		}

		/// <summary>
		/// Get translated text
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			var translateManager = Application.Ioc.Resolve<TranslateManager>();
			return translateManager.Translate(Text);
		}
	}
}
