﻿using Chat.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.Data.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("Messages");

            builder.Property(s => s.Content).IsRequired().HasMaxLength(2000);

            builder.Property(s => s.CaseId).HasMaxLength(100);

            builder.HasOne(s => s.ToRoom)
                .WithMany(m => m.Messages)
                .HasForeignKey(s => s.ToRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Case)
                .WithMany(m => m.Messages)
                .HasForeignKey(s => s.CaseId)
                ;//.OnDelete(DeleteBehavior.Cascade);
        }
    }
}
