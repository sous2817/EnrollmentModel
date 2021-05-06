namespace Semio.ClientService.OpenXml.Excel
{
    /// <summary>
    ///     Container for data needed to export an image.
    /// </summary>
    public struct ExportImage
    {
        private readonly ImageFormat _format;
        private readonly byte[] _imageData;
        private readonly int _pixelHeight;
        private readonly int _pixelWidth;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExportImage" /> struct.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="format">The format.</param>
        /// <param name="pixelWidth">Width of the pixel.</param>
        /// <param name="pixelHeight">Height of the pixel.</param>
        public ExportImage(byte[] imageData, ImageFormat format, int pixelWidth, int pixelHeight)
        {
            _imageData = imageData;
            _format = format;
            _pixelWidth = pixelWidth;
            _pixelHeight = pixelHeight;
        }

        /// <summary>
        ///     Gets the image data.
        /// </summary>
        /// <value>The image data.</value>
        public byte[] ImageData
        {
            get { return _imageData; }
        }

        /// <summary>
        ///     Gets the format.
        /// </summary>
        /// <value>The format.</value>
        public ImageFormat Format
        {
            get { return _format; }
        }

        /// <summary>
        ///     Gets the width in pixels.
        /// </summary>
        /// <value>The width.</value>
        public int PixelWidth
        {
            get { return _pixelWidth; }
        }

        /// <summary>
        ///     Gets the height in pixels.
        /// </summary>
        /// <value>The height.</value>
        public int PixelHeight
        {
            get { return _pixelHeight; }
        }
    }
}