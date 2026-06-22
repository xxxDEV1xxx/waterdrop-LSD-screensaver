using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

class LsdFluid : Form
{
    private byte[] palette;
    private Color[] colors;
    private byte[,] colorBytes;
    private Timer timer;
    private Bitmap canvas;
    private int simWidth = 160;
    private int simHeight = 100;
    private int bitmapWidth = 320;
    private int bitmapHeight = 200;
    private Point lastMousePosition;
    private float[,] density;
    private float[,] velX;
    private float[,] velY;
    private float frameTime = 0;

    // ==================== WAVE HEIGHT FIELD ====================
    private float[,] wave;
    private float[,] wavePrev;
    private float[,] waveNext;

    // ==================== FLAME PHASE ====================
    private float flamePhase = 0;

    // ==================== ORBITAL BODY ====================
    private float orbX;       // position in sim space
    private float orbY;
    private float orbVX;      // velocity
    private float orbVY;
    private float gravCX;     // gravity centre (fixed at sim centre)
    private float gravCY;

    private Random rand = new Random();

    public LsdFluid()
    {
        palette = new byte[384] {
            0x3f,0x00,0x3f, 0x3f,0x02,0x3d, 0x3f,0x04,0x3b, 0x3f,0x06,0x39,
            0x3f,0x08,0x37, 0x3f,0x0a,0x35, 0x3f,0x0c,0x33, 0x3f,0x0e,0x31,
            0x3f,0x10,0x2f, 0x3f,0x12,0x2d, 0x3f,0x14,0x2b, 0x3f,0x16,0x29,
            0x3f,0x18,0x27, 0x3f,0x1a,0x25, 0x3f,0x1c,0x23, 0x3f,0x1e,0x21,
            0x3f,0x20,0x1f, 0x3f,0x22,0x1d, 0x3f,0x24,0x1b, 0x3f,0x26,0x19,
            0x3f,0x28,0x17, 0x3f,0x2a,0x15, 0x3f,0x2c,0x13, 0x3f,0x2e,0x11,
            0x3f,0x30,0x0f, 0x3f,0x32,0x0d, 0x3f,0x34,0x0b, 0x3f,0x36,0x09,
            0x3f,0x38,0x07, 0x3f,0x3a,0x05, 0x3f,0x3c,0x03, 0x3f,0x3e,0x01,
            0x3f,0x3f,0x00, 0x3d,0x3f,0x02, 0x3b,0x3f,0x04, 0x39,0x3f,0x06,
            0x37,0x3f,0x08, 0x35,0x3f,0x0a, 0x33,0x3f,0x0c, 0x31,0x3f,0x0e,
            0x2f,0x3f,0x10, 0x2d,0x3f,0x12, 0x2b,0x3f,0x14, 0x29,0x3f,0x16,
            0x27,0x3f,0x18, 0x25,0x3f,0x1a, 0x23,0x3f,0x1c, 0x21,0x3f,0x1e,
            0x1f,0x3f,0x20, 0x1d,0x3f,0x22, 0x1b,0x3f,0x24, 0x19,0x3f,0x26,
            0x17,0x3f,0x28, 0x15,0x3f,0x2a, 0x13,0x3f,0x2c, 0x11,0x3f,0x2e,
            0x0f,0x3f,0x30, 0x0d,0x3f,0x32, 0x0b,0x3f,0x34, 0x09,0x3f,0x36,
            0x07,0x3f,0x38, 0x05,0x3f,0x3a, 0x03,0x3f,0x3c, 0x01,0x3f,0x3e,
            0x00,0x3f,0x3f, 0x00,0x3d,0x3f, 0x00,0x3b,0x3f, 0x00,0x39,0x3f,
            0x00,0x37,0x3f, 0x00,0x35,0x3f, 0x00,0x33,0x3f, 0x00,0x31,0x3f,
            0x00,0x2f,0x3f, 0x00,0x2d,0x3f, 0x00,0x2b,0x3f, 0x00,0x29,0x3f,
            0x00,0x27,0x3f, 0x00,0x25,0x3f, 0x00,0x23,0x3f, 0x00,0x21,0x3f,
            0x00,0x1f,0x3f, 0x00,0x1d,0x3f, 0x00,0x1b,0x3f, 0x00,0x19,0x3f,
            0x00,0x17,0x3f, 0x00,0x15,0x3f, 0x00,0x13,0x3f, 0x00,0x11,0x3f,
            0x00,0x0f,0x3f, 0x00,0x0d,0x3f, 0x00,0x0b,0x3f, 0x00,0x09,0x3f,
            0x00,0x07,0x3f, 0x00,0x05,0x3f, 0x00,0x03,0x3f, 0x00,0x01,0x3f,
            0x00,0x00,0x3f, 0x02,0x00,0x3f, 0x04,0x00,0x3f, 0x06,0x00,0x3f,
            0x08,0x00,0x3f, 0x0a,0x00,0x3f, 0x0c,0x00,0x3f, 0x0e,0x00,0x3f,
            0x10,0x00,0x3f, 0x12,0x00,0x3f, 0x14,0x00,0x3f, 0x16,0x00,0x3f,
            0x18,0x00,0x3f, 0x1a,0x00,0x3f, 0x1c,0x00,0x3f, 0x1e,0x00,0x3f,
            0x20,0x00,0x3f, 0x22,0x00,0x3f, 0x24,0x00,0x3f, 0x26,0x00,0x3f,
            0x28,0x00,0x3f, 0x2a,0x00,0x3f, 0x2c,0x00,0x3f, 0x2e,0x00,0x3f,
            0x30,0x00,0x3f, 0x32,0x00,0x3f, 0x34,0x00,0x3f, 0x36,0x00,0x3f,
            0x38,0x00,0x3f, 0x3a,0x00,0x3f, 0x3c,0x00,0x3f, 0x3f,0x00,0x3f
        };

        colors = new Color[128];
        colorBytes = new byte[128, 3];
        density = new float[simWidth, simHeight];
        velX = new float[simWidth, simHeight];
        velY = new float[simWidth, simHeight];

        wave     = new float[bitmapWidth, bitmapHeight];
        wavePrev = new float[bitmapWidth, bitmapHeight];
        waveNext = new float[bitmapWidth, bitmapHeight];

        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.Black;
        this.DoubleBuffered = true;
        this.TopMost = true;
        this.Bounds = SystemInformation.VirtualScreen;

        Cursor.Position = new Point(
            SystemInformation.VirtualScreen.X + SystemInformation.VirtualScreen.Width / 2,
            SystemInformation.VirtualScreen.Y + SystemInformation.VirtualScreen.Height / 2
        );
        lastMousePosition = Cursor.Position;

        canvas = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format24bppRgb);

        for (int i = 0; i < 128; i++)
        {
            int r = palette[i * 3 + 0] * 255 / 63;
            int g = palette[i * 3 + 1] * 255 / 63;
            int b = palette[i * 3 + 2] * 255 / 63;
            colors[i] = Color.FromArgb(r, g, b);
            colorBytes[i, 0] = (byte)b;
            colorBytes[i, 1] = (byte)g;
            colorBytes[i, 2] = (byte)r;
        }

        for (int y = 0; y < simHeight; y++)
        for (int x = 0; x < simWidth; x++)
        {
            float fx = x / (float)simWidth;
            float fy = y / (float)simHeight;
            density[x, y] = (float)(Math.Sin(fx * 10 + fy * 10) * 1.5 + 0.5);
            velX[x, y] = (float)Math.Sin(fx * 15) * 0.1f;
            velY[x, y] = (float)Math.Cos(fy * 15) * 0.1f;
        }

        // Gravity centre fixed at sim centre
        gravCX = simWidth  * 0.5f;
        gravCY = simHeight * 0.5f;

        // Start the comet offset from centre with a tangential velocity
        // tuned so the orbit is elliptical but stays well inside the grid.
        orbX  = gravCX + simWidth  * 0.28f;
        orbY  = gravCY - simHeight * 0.08f;
        orbVX =  0.0f;
        orbVY =  0.55f;   // tangential kick — adjust for tighter/wider orbit

        timer = new Timer();
        timer.Interval = 16;
        timer.Tick += new EventHandler(UpdateFrame);
        timer.Start();

        this.MouseMove  += new MouseEventHandler(MouseMoveHandler);
        this.MouseClick += new MouseEventHandler(MouseClickHandler);
        this.KeyDown    += new KeyEventHandler(KeyDownHandler);

        using (Graphics g = Graphics.FromImage(canvas))
            g.Clear(Color.FromArgb(palette[0], palette[1], palette[2]));
    }

    // ==================== RAIN IMPACT ====================
    private void SpawnRainImpact()
    {
        int x = rand.Next(bitmapWidth);
        int y = rand.Next(bitmapHeight);
        float energy = 8f + (float)rand.NextDouble() * 12f;

        for (int dy = -6; dy <= 6; dy++)
        for (int dx = -6; dx <= 6; dx++)
        {
            int nx = x + dx;
            int ny = y + dy;
            if (nx < 1 || ny < 1 || nx >= bitmapWidth - 1 || ny >= bitmapHeight - 1)
                continue;
            float r2 = dx * dx + dy * dy;
            wave[nx, ny] += energy * (float)Math.Exp(-r2 * 0.12f);
        }

        for (double angle = 0; angle < Math.PI * 2; angle += 0.3)
        {
            int rx = x + (int)(Math.Cos(angle) * 4);
            int ry = y + (int)(Math.Sin(angle) * 4);
            if (rx < 1 || ry < 1 || rx >= bitmapWidth - 1 || ry >= bitmapHeight - 1)
                continue;
            wave[rx, ry] -= energy * 0.3f;
        }
    }

    // ==================== WAVE PROPAGATION ====================
    private void UpdateWaves()
    {
        const float damping = 0.985f;
        const float clamp   = 24f;

        for (int y = 1; y < bitmapHeight - 1; y++)
        for (int x = 1; x < bitmapWidth - 1; x++)
        {
            waveNext[x, y] =
                (wave[x - 1, y] + wave[x + 1, y] +
                 wave[x, y - 1] + wave[x, y + 1]) * 0.5f
                - wavePrev[x, y];

            waveNext[x, y] *= damping;

            waveNext[x, y] +=
                (wave[x - 1, y] + wave[x + 1, y] +
                 wave[x, y - 1] + wave[x, y + 1]) * 0.003f;

            if (waveNext[x, y] >  clamp) waveNext[x, y] =  clamp;
            if (waveNext[x, y] < -clamp) waveNext[x, y] = -clamp;
        }

        float[,] temp = wavePrev;
        wavePrev = wave;
        wave     = waveNext;
        waveNext = temp;
    }

    private void MouseMoveHandler(object sender, MouseEventArgs e)
    {
        Point current = Cursor.Position;
        if (Math.Abs(current.X - lastMousePosition.X) > 5 ||
            Math.Abs(current.Y - lastMousePosition.Y) > 5)
            Application.Exit();
        lastMousePosition = current;
    }

    private void MouseClickHandler(object sender, MouseEventArgs e) { Application.Exit(); }
    private void KeyDownHandler(object sender, KeyEventArgs e)      { Application.Exit(); }

    protected override void OnPaint(PaintEventArgs e)
    {
        Rectangle vs = SystemInformation.VirtualScreen;
        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.DrawImage(canvas, 0, 0, vs.Width, vs.Height);
        base.OnPaint(e);
    }

    private void CreateGradientBackground(Bitmap bmp, float time)
    {
        float centerX = bitmapWidth / 2f;
        float centerY = bitmapHeight / 2f;
        float maxRadius = (float)Math.Sqrt(centerX * centerX + centerY * centerY);

        float t = time * 0.1f;
        int outerR = (int)(Math.Sin(t)     * 127 + 128);
        int outerG = (int)(Math.Sin(t + 2) * 127 + 128);
        int outerB = (int)(Math.Sin(t + 4) * 127 + 128);
        int innerR = (int)(Math.Cos(t)     * 127 + 128);
        int innerG = (int)(Math.Cos(t + 2) * 127 + 128);
        int innerB = (int)(Math.Cos(t + 4) * 127 + 128);

        BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight),
            ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        unsafe
        {
            byte* ptr = (byte*)bmpData.Scan0;
            int stride = bmpData.Stride;
            for (int y = 0; y < bitmapHeight; y++)
            {
                byte* row = ptr + y * stride;
                for (int x = 0; x < bitmapWidth; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    float tv = Math.Min(distance / maxRadius, 1f);

                    int offset = x * 3;
                    row[offset + 0] = (byte)(outerB + (innerB - outerB) * tv);
                    row[offset + 1] = (byte)(outerG + (innerG - outerG) * tv);
                    row[offset + 2] = (byte)(outerR + (innerR - outerR) * tv);
                }
            }
        }
        bmp.UnlockBits(bmpData);
    }

    private void UpdateFrame(object sender, EventArgs e)
    {
        float t = frameTime * 0.35f;

        // ==================== GRAVITATIONAL ORBIT ====================
        // Newtonian inverse-square gravity toward a fixed centre point.
        // GM tuned so a circular orbit at the starting radius takes ~6-8
        // seconds at 60fps — slow, weighty, natural-feeling.
        const float GM        = 18.0f;   // gravitational parameter
        const float softening = 4.0f;    // prevents singularity at close approach
        const float drag      = 0.9995f; // very light damping — orbit decays slowly
                                         // into tightening spiral over time

        float dxG = gravCX - orbX;
        float dyG = gravCY - orbY;
        float distSq  = dxG * dxG + dyG * dyG + softening * softening;
        float distInv = 1.0f / (float)Math.Sqrt(distSq);
        float force   = GM * distInv * distInv;   // 1/r² magnitude

        // Accelerate toward centre
        orbVX += dxG * distInv * force;
        orbVY += dyG * distInv * force;

        // Tiny drag so a perfectly circular orbit drifts inward over ~30s,
        // then the slight eccentricity from the starting conditions causes it
        // to swing back out — natural precessing ellipse.
        orbVX *= drag;
        orbVY *= drag;

        orbX += orbVX;
        orbY += orbVY;

        // Hard border reflect — keeps it on screen if eccentricity grows large
        float margin = 6f;
        if (orbX < margin)             { orbX = margin;             orbVX =  Math.Abs(orbVX) * 0.6f; }
        if (orbX > simWidth - margin)  { orbX = simWidth - margin;  orbVX = -Math.Abs(orbVX) * 0.6f; }
        if (orbY < margin)             { orbY = margin;             orbVY =  Math.Abs(orbVY) * 0.6f; }
        if (orbY > simHeight - margin) { orbY = simHeight - margin; orbVY = -Math.Abs(orbVY) * 0.6f; }

        int cx = (int)orbX;
        int cy = (int)orbY;

        // ==================== TURBULENT FLAME MASK ====================
        int radius = 8;

        for (int dy = -radius; dy <= radius; dy++)
        for (int dx = -radius; dx <= radius; dx++)
        {
            int nx = cx + dx;
            int ny = cy + dy;

            if (nx < 0 || ny < 0 || nx >= simWidth || ny >= simHeight)
                continue;

            float dist  = (float)Math.Sqrt(dx * dx + dy * dy);
            float angle = (float)Math.Atan2(dy, dx);

            // Jagged flame boundary: three superimposed angular frequencies
            float flameEdge =
                radius
                + (float)Math.Sin(angle * 5  + flamePhase)         * 2.5f
                + (float)Math.Sin(angle * 9  - flamePhase * 1.7f)  * 1.5f
                + (float)Math.Sin(angle * 13 + flamePhase * 0.7f)  * 0.8f;

            if (dist > flameEdge)
                continue;

            float falloff = 1.0f - dist / flameEdge;
            falloff *= falloff;

            density[nx, ny] += 0.8f * falloff;

            // Swirling tangential injection
            float swirl = (float)Math.Sin(angle * 7 + flamePhase * 3);
            velX[nx, ny] += -dy * 0.03f * swirl * falloff;
            velY[nx, ny] +=  dx * 0.03f * swirl * falloff;
        }

        // ==================== ROLLING VORTEX CORE ====================
        for (int dy = -10; dy <= 10; dy++)
        for (int dx = -10; dx <= 10; dx++)
        {
            int nx = cx + dx;
            int ny = cy + dy;

            if (nx < 1 || ny < 1 || nx >= simWidth - 1 || ny >= simHeight - 1)
                continue;

            float r2 = dx * dx + dy * dy;
            if (r2 > 100f) continue;

            float inv = 1.0f / (1.0f + r2 * 0.08f);
            velX[nx, ny] += -dy * inv * 0.05f;
            velY[nx, ny] +=  dx * inv * 0.05f;
        }

        // Advance flame phase each frame
        flamePhase += 0.18f;

        // ==================== FLUID ADVECTION (unchanged) ====================
        float[,] newDensity = new float[simWidth, simHeight];
        for (int y = 1; y < simHeight - 1; y++)
        for (int x = 1; x < simWidth - 1; x++)
        {
            float px = x - velX[x, y];
            float py = y - velY[x, y];
            int ix = (int)px, iy = (int)py;
            float fx = px - ix, fy = py - iy;
            ix = Math.Max(1, Math.Min(simWidth - 2, ix));
            iy = Math.Max(1, Math.Min(simHeight - 2, iy));

            float d = (1 - fx) * (1 - fy) * density[ix,     iy    ]
                    + fx       * (1 - fy) * density[ix + 1, iy    ]
                    + (1 - fx) * fy       * density[ix,     iy + 1]
                    + fx       * fy       * density[ix + 1, iy + 1];
            newDensity[x, y] = Math.Max(0, Math.Min(1, d));
        }
        density = newDensity;

        for (int y = 1; y < simHeight - 1; y++)
        for (int x = 1; x < simWidth - 1; x++)
        {
            density[x, y] = (density[x, y] +
                             (density[x-1,y] + density[x+1,y] +
                              density[x,y-1] + density[x,y+1]) * 0.2f) * 0.5f;
        }

        // ---- Localised smoothing: 3 passes, radius 14, circular mask ----
        // Applied only around the comet centre so the rest of the field
        // keeps its sharp psychedelic character.
        for (int pass = 0; pass < 3; pass++)
        {
            for (int y = Math.Max(1, cy - 14); y <= Math.Min(simHeight - 2, cy + 14); y++)
            for (int x = Math.Max(1, cx - 14); x <= Math.Min(simWidth  - 2, cx + 14); x++)
            {
                int ddx = x - cx;
                int ddy = y - cy;
                if (ddx * ddx + ddy * ddy > 196) continue;  // r=14 circle
                density[x, y] =
                    (density[x - 1, y] + density[x + 1, y] +
                     density[x, y - 1] + density[x, y + 1]) * 0.25f;
            }
        }

        // --- Step 1: render gradient background into canvas ---
        CreateGradientBackground(canvas, frameTime);

        // --- Step 2: snapshot the background for UV sampling ---
        Bitmap background = (Bitmap)canvas.Clone();

        // --- Step 3: lock both bitmaps and apply refraction + fluid blend ---
        BitmapData dstData = canvas.LockBits(
            new Rectangle(0, 0, bitmapWidth, bitmapHeight),
            ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        BitmapData srcData = background.LockBits(
            new Rectangle(0, 0, bitmapWidth, bitmapHeight),
            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

        unsafe
        {
            byte* dst     = (byte*)dstData.Scan0;
            byte* src     = (byte*)srcData.Scan0;
            int dstStride = dstData.Stride;
            int srcStride = srcData.Stride;

            for (int y = 0; y < bitmapHeight; y++)
            {
                byte* dstRow = dst + y * dstStride;

                for (int x = 0; x < bitmapWidth; x++)
                {
                    int xm = Math.Max(0, x - 1);
                    int xp = Math.Min(bitmapWidth - 1, x + 1);
                    int ym = Math.Max(0, y - 1);
                    int yp = Math.Min(bitmapHeight - 1, y + 1);

                    float wnx = wave[xp, y] - wave[xm, y];
                    float wny = wave[x, yp] - wave[x, ym];

                    int rx = x + (int)(wnx * 1.8f);
                    int ry = y + (int)(wny * 1.8f);
                    if (rx < 0) rx = 0;
                    if (ry < 0) ry = 0;
                    if (rx >= bitmapWidth)  rx = bitmapWidth  - 1;
                    if (ry >= bitmapHeight) ry = bitmapHeight - 1;

                    byte* srcPx = src + ry * srcStride + rx * 3;
                    byte bgB = srcPx[0];
                    byte bgG = srcPx[1];
                    byte bgR = srcPx[2];

                    // Bilinear sample of density at this bitmap pixel.
                    // Eliminates the 2x2 block pattern from integer nearest-neighbor.
                    float sxf = x * (simWidth  - 1) / (float)(bitmapWidth  - 1);
                    float syf = y * (simHeight - 1) / (float)(bitmapHeight - 1);
                    int   sx0 = (int)sxf;
                    int   sy0 = (int)syf;
                    int   sx1 = Math.Min(sx0 + 1, simWidth  - 1);
                    int   sy1 = Math.Min(sy0 + 1, simHeight - 1);
                    float sfx = sxf - sx0;
                    float sfy = syf - sy0;
                    float d =
                        density[sx0, sy0] * (1 - sfx) * (1 - sfy) +
                        density[sx1, sy0] *      sfx  * (1 - sfy) +
                        density[sx0, sy1] * (1 - sfx) *      sfy  +
                        density[sx1, sy1] *      sfx  *      sfy;

                    byte colorIndex = (byte)((d + 0.2f * (float)Math.Sin(t * 0.05f)) * 127);
                    byte fluidR = colorBytes[colorIndex % 128, 2];
                    byte fluidG = colorBytes[colorIndex % 128, 1];
                    byte fluidB = colorBytes[colorIndex % 128, 0];

                    float alpha = d;
                    int offset = x * 3;
                    dstRow[offset + 2] = (byte)(fluidR * alpha + bgR * (1 - alpha));
                    dstRow[offset + 1] = (byte)(fluidG * alpha + bgG * (1 - alpha));
                    dstRow[offset + 0] = (byte)(fluidB * alpha + bgB * (1 - alpha));
                }
            }
        }

        canvas.UnlockBits(dstData);
        background.UnlockBits(srcData);
        background.Dispose();

        frameTime += 1.0f;

        // --- Step 4: rain impacts + wave propagation ---
        if (rand.Next(100) < 40)
            SpawnRainImpact();
        UpdateWaves();

        this.Invalidate();
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new LsdFluid());
    }
}
