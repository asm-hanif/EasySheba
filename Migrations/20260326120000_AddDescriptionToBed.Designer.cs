using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using EasySheba.Data;

#nullable disable

namespace EasySheba.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260326120000_AddDescriptionToBed")]
    partial class AddDescriptionToBed
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            // Intentionally minimal -- EF tooling not required for manual migration file
        }
    }
}
