namespace Intalio.Tools.Common.Objects
{
    public class ImageRectangle
    {
        public double X1 { get; set; }
        public double X2 { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }

        public ImageRectangle()
        {
            
        }

        public ImageRectangle(double[] rectangle)
        {
            if (rectangle.Length >=1)
            {
                X1 = rectangle[0];
            }
            if (rectangle.Length >=2)
            {
                Y1 = rectangle[1];
            }
            if (rectangle.Length >=3)
            {
                X2 = rectangle[2];
            }
            if (rectangle.Length >=4)
            {
                Y2 = rectangle[3];
            }
        }
    }
}
