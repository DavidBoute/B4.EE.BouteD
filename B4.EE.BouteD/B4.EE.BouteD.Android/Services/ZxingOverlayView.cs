using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Text;
using Android.Views;
using ZXing.Mobile;

namespace B4.EE.BouteD.Droid.Services
{
    // Custom Overlay voor gebruik in Barcodescanner
    // Aangepast van
    // http://slackshotindustries.blogspot.be/2013/04/creating-custom-overlays-in-xzing.html
    class ZxingOverlayView : View
    {
        private Paint defaultPaint;
        private Paint pressedPaint;
        private bool hasTorch = false;
        private bool torchOn = false;
        private bool cancelPressed = false;
        private Color buttonFontColor;
        private Color buttonColor;
        private MobileBarcodeScanner scanner;
        private Context context;

        public ZxingOverlayView(Context context, MobileBarcodeScanner scanner) : base(context)
        {
            this.context = context;
            this.scanner = scanner;

            // Determine if the device has Torch functionality
            hasTorch = this.Context.PackageManager.HasSystemFeature(PackageManager.FeatureCameraFlash);

            // Initialize these once for performance rather than calling them every time in onDraw()
            defaultPaint = new Paint(PaintFlags.AntiAlias);
            pressedPaint = new Paint(PaintFlags.AntiAlias);
            pressedPaint.Color = new Color(96, 97, 104);
            buttonFontColor = Color.White;
            buttonColor = Color.Black;
        }

        Rect GetCancelButtonRect()
        {
            var metrics = Resources.DisplayMetrics;
            int width = metrics.WidthPixels * 7 / 16;
            int height = metrics.HeightPixels * 2 / 9;
            int leftOffset = metrics.WidthPixels / 22;
            int topOffset = metrics.HeightPixels * 3 / 4;
            var cancelRect = new Rect(leftOffset, topOffset, leftOffset + width, topOffset + height);

            return cancelRect;
        }

        Rect GetTorchButtonRect()
        {
            var metrics = Resources.DisplayMetrics;
            int width = metrics.WidthPixels * 7 / 16;
            int height = metrics.HeightPixels * 2 / 9;
            int leftOffset = metrics.WidthPixels * 33 / 64;
            int topOffset = metrics.HeightPixels * 3 / 4; 
            var torchRect = new Rect(leftOffset, topOffset, leftOffset + width, topOffset + height);

            return torchRect;
        }

        protected override void OnDraw(Canvas canvas)
        {
            var scale = Resources.DisplayMetrics.Density;

            var cancelBtn = GetCancelButtonRect();
            var torchBtn = GetTorchButtonRect();

            var width = canvas.Width;
            var height = canvas.Height;
            var textPaint = new TextPaint();
            textPaint.Color = buttonFontColor;
            textPaint.AntiAlias = true;
            textPaint.BgColor = Color.Gray;
            textPaint.TextSize = 16 * scale;

            //Draw button outlines
            defaultPaint.Color = buttonFontColor;
            defaultPaint.Alpha = 255;
            pressedPaint.Color = Color.Black;
            canvas.DrawRect(cancelBtn.Left + 1, cancelBtn.Top + 1, cancelBtn.Right + 1, cancelBtn.Bottom + 1, cancelPressed ? pressedPaint : defaultPaint);

            if (hasTorch)
            {
                canvas.DrawRect(torchBtn.Left + 1, torchBtn.Top + 1, torchBtn.Right + 1, torchBtn.Bottom + 1, torchOn ? pressedPaint : defaultPaint);
            }
                
            //Draw buttons
            defaultPaint.Color = buttonColor;
            defaultPaint.Alpha = 255;
            pressedPaint.Color = new Color(96, 97, 104);
            canvas.DrawRect(cancelBtn, cancelPressed ? pressedPaint : defaultPaint);

            if (hasTorch)
            {
                canvas.DrawRect(torchBtn, torchOn ? pressedPaint : defaultPaint);
            }

            //Draw button text
            var btnText = new StaticLayout(scanner.CancelButtonText, textPaint, cancelBtn.Width(), Android.Text.Layout.Alignment.AlignCenter, 1.0f, 0.0f, false);
            canvas.Save();
            canvas.Translate(cancelBtn.Left, cancelBtn.Top + (cancelBtn.Height() / 3) + (btnText.Height / 2));
            btnText.Draw(canvas);
            canvas.Restore();

            if (hasTorch)
            {
                btnText = new StaticLayout(scanner.FlashButtonText, textPaint, torchBtn.Width(), Android.Text.Layout.Alignment.AlignCenter, 1.0f, 0.0f, false);
                canvas.Save();
                canvas.Translate(torchBtn.Left, torchBtn.Top + (torchBtn.Height() / 3) + (btnText.Height / 2));
                btnText.Draw(canvas);
                canvas.Restore();
            }
        }

        public override bool OnTouchEvent(MotionEvent me)
        {
            if (me.Action == MotionEventActions.Down)
            {
                if (GetCancelButtonRect().Contains((int)me.RawX, (int)me.RawY))
                {
                    cancelPressed = true;
                    this.Invalidate();
                    scanner.Cancel();
                }
                else if (GetTorchButtonRect().Contains((int)me.RawX, (int)me.RawY))
                {
                    scanner.ToggleTorch();
                    torchOn = !torchOn;
                    this.Invalidate();
                }
                return true;
            }
            else
                return false;
        }
    }
}
