﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyBuilding.Tests.Enums;
using System;

namespace SkyBuilding.Tests
{
    [TestClass]
    public class EnumExtensions
    {
        [TestMethod]
        public void GetText()
        {
            var text = RoleEnum.User.GetText();

            var text2 = RoleEnum.Admin.GetText();
        }

        [TestMethod]
        public void GetTextFlags()
        {
            var @enum = RoleFlagsEnum.User | RoleFlagsEnum.Admin;

            var text = @enum.GetText();
        }
    }
}
