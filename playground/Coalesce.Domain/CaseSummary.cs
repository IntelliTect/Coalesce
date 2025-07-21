﻿using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Coalesce.Domain;

public class CaseSummary
{
    public int CaseSummaryId { get; set; }
    public int OpenCases { get; set; }
    public int CaseCount { get; set; }
    public int CloseCases { get; set; }

    [ListText]
    public string? Description { get; set; }

    public IDictionary<string, int> TestDict { get; set; } = new Dictionary<string, int>();

    [Coalesce]
    public static CaseSummary GetCaseSummary(AppDbContext db)
    {
        return new CaseSummary()
        {
            CaseSummaryId = 1,
            OpenCases = 2,
            CaseCount = db.Cases.Count(),
            CloseCases = 4,
            Description = "This is a test!"
        };
    }
}
