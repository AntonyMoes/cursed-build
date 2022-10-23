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
            return serialized switch {
                "Client" => Department.CLI,
                "VFX" => Department.VFX,
                "PM" => Department.PM,
                "3D" => Department._3D,
                _ => throw new ArgumentOutOfRangeException(nameof(serialized), serialized, null)
            };
        }
    }
}