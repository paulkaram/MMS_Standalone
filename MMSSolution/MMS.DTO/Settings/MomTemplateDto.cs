using System.Text.Json.Serialization;

namespace MMS.DTO.Settings
{
    public class MomTemplateDto
    {
        public int Id { get; set; }
        public int? BranchId { get; set; }
        public string? BranchNameAr { get; set; }
        public string? BranchNameEn { get; set; }
        public int TemplateType { get; set; }
        public string TemplateTypeName { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public MomTemplateConfigDto Config { get; set; } = null!;
        public string? HtmlTemplate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class MomTemplateListItemDto
    {
        public int Id { get; set; }
        public int? BranchId { get; set; }
        public string? BranchNameAr { get; set; }
        public string? BranchNameEn { get; set; }
        public int TemplateType { get; set; }
        public string TemplateTypeName { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class MomTemplateCreateDto
    {
        public int? BranchId { get; set; }
        public int TemplateType { get; set; }
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public MomTemplateConfigDto Config { get; set; } = null!;
        public string? HtmlTemplate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; }
    }

    public class MomTemplateUpdateDto
    {
        public int Id { get; set; }
        public int? BranchId { get; set; }
        public int TemplateType { get; set; }
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public MomTemplateConfigDto Config { get; set; } = null!;
        public string? HtmlTemplate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
    }

    public class MomTemplateConfigDto
    {
        [JsonPropertyName("colors")]
        public MomTemplateColorsDto Colors { get; set; } = new();

        [JsonPropertyName("fonts")]
        public MomTemplateFontsDto Fonts { get; set; } = new();

        [JsonPropertyName("pageLayout")]
        public MomTemplatePageLayoutDto PageLayout { get; set; } = new();

        [JsonPropertyName("sections")]
        public Dictionary<string, MomTemplateSectionDto> Sections { get; set; } = new();

        [JsonPropertyName("labels")]
        public Dictionary<string, string> Labels { get; set; } = new();

        [JsonPropertyName("roles")]
        public Dictionary<string, string> Roles { get; set; } = new();

        [JsonPropertyName("tableColumns")]
        public MomTemplateTableColumnsDto TableColumns { get; set; } = new();
    }

    public class MomTemplateColorsDto
    {
        [JsonPropertyName("primary")]
        public string Primary { get; set; } = "#803580";

        [JsonPropertyName("secondary")]
        public string Secondary { get; set; } = "#F5F5FA";

        [JsonPropertyName("border")]
        public string Border { get; set; } = "#C085C0";

        [JsonPropertyName("text")]
        public string Text { get; set; } = "#000000";

        [JsonPropertyName("mutedText")]
        public string MutedText { get; set; } = "#646464";

        [JsonPropertyName("success")]
        public string Success { get; set; } = "#228B22";

        [JsonPropertyName("danger")]
        public string Danger { get; set; } = "#DC3545";

        [JsonPropertyName("white")]
        public string White { get; set; } = "#FFFFFF";
    }

    public class MomTemplateFontsDto
    {
        [JsonPropertyName("arabicFont")]
        public string ArabicFont { get; set; } = "Tajawal";

        [JsonPropertyName("fallbackFont")]
        public string FallbackFont { get; set; } = "Tajawal";

        [JsonPropertyName("titleSize")]
        public int TitleSize { get; set; } = 24;

        [JsonPropertyName("headingSize")]
        public int HeadingSize { get; set; } = 14;

        [JsonPropertyName("bodySize")]
        public int BodySize { get; set; } = 12;

        [JsonPropertyName("smallSize")]
        public int SmallSize { get; set; } = 10;
    }

    public class MomTemplatePageLayoutDto
    {
        [JsonPropertyName("topMargin")]
        public int TopMargin { get; set; } = 40;

        [JsonPropertyName("bottomMargin")]
        public int BottomMargin { get; set; } = 40;

        [JsonPropertyName("leftMargin")]
        public int LeftMargin { get; set; } = 40;

        [JsonPropertyName("rightMargin")]
        public int RightMargin { get; set; } = 40;

        [JsonPropertyName("rtl")]
        public bool Rtl { get; set; } = true;
    }

    public class MomTemplateSectionDto
    {
        [JsonPropertyName("visible")]
        public bool Visible { get; set; } = true;

        [JsonPropertyName("order")]
        public int Order { get; set; }
    }

    public class MomTemplateTableColumnsDto
    {
        [JsonPropertyName("attendeesIndexWidth")]
        public int AttendeesIndexWidth { get; set; } = 30;

        [JsonPropertyName("attendeesNameWidth")]
        public int AttendeesNameWidth { get; set; } = 150;

        [JsonPropertyName("attendeesJobTitleWidth")]
        public int AttendeesJobTitleWidth { get; set; } = 150;

        [JsonPropertyName("attendeesRoleWidth")]
        public int AttendeesRoleWidth { get; set; } = 70;

        [JsonPropertyName("attendeesStatusWidth")]
        public int AttendeesStatusWidth { get; set; } = 60;

        [JsonPropertyName("recommendationsIndexWidth")]
        public int RecommendationsIndexWidth { get; set; } = 25;

        [JsonPropertyName("recommendationsTextWidth")]
        public int RecommendationsTextWidth { get; set; } = 200;

        [JsonPropertyName("recommendationsAgendaWidth")]
        public int RecommendationsAgendaWidth { get; set; } = 70;

        [JsonPropertyName("recommendationsOwnerWidth")]
        public int RecommendationsOwnerWidth { get; set; } = 80;

        [JsonPropertyName("recommendationsDueDateWidth")]
        public int RecommendationsDueDateWidth { get; set; } = 80;
    }

    public class MomTemplateTypeDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
    }
}
