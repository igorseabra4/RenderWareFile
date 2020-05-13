namespace RenderWareFile.Sections
{
    public struct Matrix3x3
    {
        public float M11;
        public float M12;
        public float M13;
        public float M21;
        public float M22;
        public float M23;
        public float M31;
        public float M32;
        public float M33;

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