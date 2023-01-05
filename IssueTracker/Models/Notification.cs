﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace IssueTracker.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        [Required]
        [DisplayName("Title")]
        public string Title { get; set; }

        [Required]
        [DisplayName("Message")]
        public string Message { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date")]
        public DateTimeOffset CreatedOn { get; set; }

        [Required]
        [DisplayName("Recipient")]
        public string RecipientId { get; set; }

        [Required]
        [DisplayName("Sender")]
        public string SenderId { get; set; }

        [DisplayName("Has been viewed")]
        public bool Viewed { get; set; }


        // Navigation Properties
        public virtual Ticket Ticket { get; set; }

        public virtual IssueTrackerUser Recipient { get; set; }

        public virtual IssueTrackerUser Sender { get; set; }

    }
}
