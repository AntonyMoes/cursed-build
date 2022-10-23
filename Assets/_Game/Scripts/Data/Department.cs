using System;

namespace _Game.Scripts.Data {
    public enum Department {
        CLI,
        VFX,
        PM,
        _3D
    }

    public static class DepartmentHelper {
        public static Department ToDepartment(this string serialized) {
            if (!Enum.TryParse(serialized, out Department department)) {
                throw new ArgumentOutOfRangeException(nameof(serialized), serialized, null);
            }

            return department;
        }
    }
}