using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RenderWareFile.Sections
{
    public class MaterialList_0008 : RWSection
    {
        public MaterialListStruct_0001 materialListStruct { get; internal set; }
        public Material_0007[] materialList { get; set; }

        public MaterialList_0008(Material_0007[] materialList)
        {
            this.materialListStruct = new MaterialListStruct_0001(materialList.Length);
            this.materialList = materialList;
        }

        public MaterialList_0008(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.MaterialList;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            int startOfSection = (int)binaryReader.BaseStream.Position;

            Section materialListStructSection = (Section)binaryReader.ReadInt32();
            if (materialListStructSection != Section.Struct) throw new Exception();
            materialListStruct = new MaterialListStruct_0001(binaryReader);

#if DEBUG
            List<Material_0007> materials = new List<Material_0007>();
            for (int i = 0; binaryReader.BaseStream.Position < startOfSection + sectionSize; i++)
            {
                Section materialSection = (Section)binaryReader.ReadInt32();
                if (materialSection != Section.Material) throw new Exception();
                materials.Add(new Material_0007(binaryReader));
            }
            materialList = materials.ToArray();
            materialListStruct.materialCount = materialList.Length;
#else
            materialList = new Material_0007[materialListStruct.materialCount];
            for (int i = 0; i < materialListStruct.materialCount; i++)
            {
                Section materialSection = (Section)binaryReader.ReadInt32();
                if (materialSection != Section.Material) throw new Exception();
                materialList[i] = new Material_0007(binaryReader);
            }
#endif
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.MaterialList;

            listBytes.AddRange(materialListStruct.GetBytes(fileVersion));
            for (int i = 0; i < materialList.Count(); i++)
            {
                listBytes.AddRange(materialList[i].GetBytes(fileVersion));
            }
        }
    }
}
