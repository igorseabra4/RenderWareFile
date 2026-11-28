namespace RenderWareFile.Sections
{
    public struct Matrix3x3
    {
        public float M11 { get; set; }
        public float M12 { get; set; }
        public float M13 { get; set; }
        public float M21 { get; set; }
        public float M22 { get; set; }
        public float M23 { get; set; }
        public float M31 { get; set; }
        public float M32 { get; set; }
        public float M33 { get; set; }

        public static Matrix3x3 Identity => new Matrix3x3()
        {
            M11 = 1f,
            M12 = 0f,
            M13 = 0f,
            M21 = 0f,
            M22 = 1f,
            M23 = 0f,
            M31 = 0f,
            M32 = 0f,
            M33 = 1f,
        };
    }
}