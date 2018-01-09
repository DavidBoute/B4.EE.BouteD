using Android.Content;
using Android.Graphics;
using Android.Widget;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: Xamarin.Forms.ExportRenderer(typeof(Xamarin.Forms.Switch),
typeof(B4.EE.BouteD.Droid.CustomRenderers.CustomSwitchRenderer))]

namespace B4.EE.BouteD.Droid.CustomRenderers
{
    public class CustomSwitchRenderer : SwitchRenderer
    {
        private Android.Graphics.Color redColor = new Android.Graphics.Color(215, 32, 32);
        private Android.Graphics.Color greenColor = new Android.Graphics.Color(32, 156, 68);

        protected override void Dispose(bool disposing)
        {
            this.Control.CheckedChange -= this.OnCheckedChange;
            base.Dispose(disposing);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Switch> e)
        {
            base.OnElementChanged(e);

            if (this.Control != null)
            {
                this.Control.TextOn = "ja";
                this.Control.TextOff = "nee";
                this.Control.ShowText = true;

                if (this.Control.Checked)
                {
                    this.Control.ThumbDrawable.SetColorFilter(greenColor, PorterDuff.Mode.SrcAtop);
                }
                else
                {
                    this.Control.ThumbDrawable.SetColorFilter(redColor, PorterDuff.Mode.SrcAtop);
                }

                this.Control.CheckedChange += this.OnCheckedChange;
            }
        }

        private void OnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (this.Control.Checked)
            {
                this.Control.ThumbDrawable.SetColorFilter(greenColor, PorterDuff.Mode.SrcAtop);
                this.Element.IsToggled = true;
            }
            else
            {
                this.Control.ThumbDrawable.SetColorFilter(redColor, PorterDuff.Mode.SrcAtop);
                this.Element.IsToggled = false;
            }
        }

        public CustomSwitchRenderer(Context context) : base(context)
        {

        }
    }
}