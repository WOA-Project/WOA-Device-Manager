using System.Collections.Generic;
using System;

namespace WOADeviceManager.Pages
{
    public class BootDevice
    {
        public uint SectorCount
        {
            get; set;
        }

        public uint SectorSize
        {
            get; set;
        }

        public ushort FlashType
        {
            get; set;
        }

        public ushort FlashIndex
        {
            get; set;
        }

        public uint Unknown
        {
            get; set;
        }

        public string DevicePath
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public BootDevice(uint SectorCount, uint SectorSize, ushort FlashType, ushort FlashIndex, uint Unknown, string DevicePath)
        {
            this.SectorCount = SectorCount;
            this.SectorSize = SectorSize;
            this.FlashType = FlashType;
            this.FlashIndex = FlashIndex;
            this.Unknown = Unknown;
            this.DevicePath = DevicePath;
            this.Name = FormatDevicePath(DevicePath);
        }

        private readonly static Guid EmmcUserPartitionGuid = new("B615F1F5-5088-43CD-809C-A16E52487D00");
        private readonly static Guid EmmcBootPartition1Guid = new("12C55B20-25D3-41C9-8E06-282D94C676AD");
        private readonly static Guid EmmcBootPartition2Guid = new("6B76A6DB-0257-48A9-AA99-F6B1655F7B00");
        private readonly static Guid EmmcRpmbPartitionGuid = new("C49551EA-D6BC-4966-9499-871E393133CD");
        private readonly static Guid EmmcGppPartition1Guid = new("B9251EA5-3462-4807-86C6-8948B1B36163");
        private readonly static Guid EmmcGppPartition2Guid = new("24F906CD-EE11-43E1-8427-DC7A36F4C059");
        private readonly static Guid EmmcGppPartition3Guid = new("5A5709A9-AC40-4F72-8862-5B0104166E76");
        private readonly static Guid EmmcGppPartition4Guid = new("A44E27C9-258E-406E-BF33-77F5F244C487");
        private readonly static Guid SdRemovableGuid = new("D1531D41-3F80-4091-8D0A-541F59236D66");
        private readonly static Guid UfsLU0Guid = new("860845C1-BE09-4355-8BC1-30D64FF8E63A");
        private readonly static Guid UfsLU1Guid = new("8D90D477-39A3-4A38-AB9E-586FF69ED051");
        private readonly static Guid UfsLU2Guid = new("EDF85868-87EC-4F77-9CDA-5F10DF2FE601");
        private readonly static Guid UfsLU3Guid = new("1AE69024-8AEB-4DF8-BC98-0032DBDF5024");
        private readonly static Guid UfsLU4Guid = new("D33F1985-F107-4A85-BE38-68DC7AD32CEA");
        private readonly static Guid UfsLU5Guid = new("4BA1D05F-088E-483F-A97E-B19B9CCF59B0");
        private readonly static Guid UfsLU6Guid = new("4ACF98F6-26FA-44D2-8132-282F2D19A4C5");
        private readonly static Guid UfsLU7Guid = new("8598155F-34DE-415C-8B55-843E3322D36F");
        private readonly static Guid UfsRPMBGuid = new("5397474E-F75D-44B3-8E57-D9324FCF6FE1");

        private static Dictionary<Guid, string> guidNames = new Dictionary<Guid, string>()
        {
            { EmmcUserPartitionGuid, "eMMC (User)" },
            { EmmcBootPartition1Guid, "eMMC (Boot 1)" },
            { EmmcBootPartition2Guid, "eMMC (Boot 2)" },
            { EmmcRpmbPartitionGuid, "eMMC (RPMB)" },
            { EmmcGppPartition1Guid, "eMMC (GPP 1)" },
            { EmmcGppPartition2Guid, "eMMC (GPP 2)" },
            { EmmcGppPartition3Guid, "eMMC (GPP 3)" },
            { EmmcGppPartition4Guid, "eMMC (GPP 4)" },
            { SdRemovableGuid, "SD Card (Removable)" },
            { UfsLU0Guid, "UFS (LUN 0)" },
            { UfsLU1Guid, "UFS (LUN 1)" },
            { UfsLU2Guid, "UFS (LUN 2)" },
            { UfsLU3Guid, "UFS (LUN 3)" },
            { UfsLU4Guid, "UFS (LUN 4)" },
            { UfsLU5Guid, "UFS (LUN 5)" },
            { UfsLU6Guid, "UFS (LUN 6)" },
            { UfsLU7Guid, "UFS (LUN 7)" },
            { UfsRPMBGuid, "UFS (RPMB)" },
        };

        public static string FormatDevicePath(string DevicePath)
        {
            if (DevicePath.Contains("VenHw("))
            {
                string DevicePathGuidBit = DevicePath.Split("(")[^1].Split(")")[0];
                if (Guid.TryParse(DevicePathGuidBit, out Guid DevicePathGuid))
                {
                    if (guidNames.TryGetValue(DevicePathGuid, out string FriendlyDesc))
                    {
                        return $"{FriendlyDesc}";
                    }
                }
            }

            return "Unknown";
        }
    }
}
