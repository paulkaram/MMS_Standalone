-- MMS Dictionary INSERT script
-- Uses N prefix for Unicode

IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Abbreviation')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Abbreviation', N'Abbreviation', N'الاختصار');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Absent')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Absent', N'Absent', N'غائب');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ActionName')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ActionName', N'Action Name', N'اسم الإجراء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Actions')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Actions', N'Actions', N'إجراءات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Active')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Active', N'Active', N'نشط');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ActiveRoles')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ActiveRoles', N'Active Roles', N'أدوار نشطة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ActiveTemplates')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ActiveTemplates', N'Active Templates', N'القوالب النشطة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ActiveTypes')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ActiveTypes', N'Active Types', N'أنواع نشطة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ActiveUsers')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ActiveUsers', N'Active Users', N'مستخدمين نشطين');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Activities')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Activities', N'Activities', N'الأنشطة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ActivityNumber')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ActivityNumber', N'Activity Number', N'رقم النشاط');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ActivityWillAppear')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ActivityWillAppear', N'Activity Will Appear', N'ستظهر الأنشطة هنا عند حدوثها');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Add')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Add', N'Add', N'إضافة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddActivity')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddActivity', N'Add Activity', N'إضافة نشاط');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddAgendaItem')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddAgendaItem', N'Add Agenda Item', N'إضافة بند');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddAtLeastOneItem')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddAtLeastOneItem', N'Add At Least One Item', N'أضف بنداً واحداً على الأقل لمتابعة إنشاء الجلسة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddAttachment')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddAttachment', N'Add Attachment', N'إضافة مرفق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddDepartment')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddDepartment', N'Add Department', N'إضافة قسم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddEntry')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddEntry', N'Add Entry', N'إضافة مصطلح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddFirstItem')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddFirstItem', N'Add First Item', N'إضافة أول بند');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddFirstRole')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddFirstRole', N'Add First Role', N'أضف أول دور للبدء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddFirstUser')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddFirstUser', N'Add First User', N'أضف أول مستخدم للبدء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddFirstVotingType')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddFirstVotingType', N'Add First Voting Type', N'أضف أول نوع تصويت للبدء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddItem')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddItem', N'Add Item', N'إضافة بند');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddMeeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddMeeting', N'Add Meeting', N'إضافة اجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddMember')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddMember', N'Add Member', N'إضافة عضو');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddNew')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddNew', N'Add New', N'إضافة جديد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddNote')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddNote', N'Add Note', N'إضافة ملاحظة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddRole')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddRole', N'Add Role', N'إضافة دور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddSuccess')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddSuccess', N'Add Success', N'تمت الإضافة بنجاح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddTask')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddTask', N'Add Task', N'إضافة مهمة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddTemplate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddTemplate', N'Add Template', N'إضافة قالب');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddTopic')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddTopic', N'Add Topic', N'إضافة محور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddUser')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddUser', N'Add User', N'إضافة مستخدم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddUserToDepartment')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddUserToDepartment', N'Add User To Department', N'إضافة مستخدم للقسم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AddVotingType')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AddVotingType', N'Add Voting Type', N'إضافة نوع تصويت');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AdditionalMemberName')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AdditionalMemberName', N'Additional Member Name', N'اسم العضو الإضافي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Admin')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Admin', N'Admin', N'المسؤول');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AdvancedSearch')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AdvancedSearch', N'Advanced Search', N'بحث متقدم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Agenda')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Agenda', N'Agenda', N'جدول الأعمال');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AgendaItems')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AgendaItems', N'Agenda Items', N'بنود جدول الأعمال');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AgendaNotes')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AgendaNotes', N'Agenda Notes', N'ملاحظة على جدول الأعمال');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'All')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'All', N'All', N'All');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AllBranches')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AllBranches', N'All Branches', N'جميع الفروع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AllCategories')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AllCategories', N'All Categories', N'جميع الفئات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AllCaughtUp')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AllCaughtUp', N'All Caught Up', N'لا توجد توصيات عاجلة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AllMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AllMeetings', N'All Meetings', N'الكل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AllTasksCompleted')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AllTasksCompleted', N'All Tasks Completed', N'تم إنجاز جميع المهام');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AllTypes')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AllTypes', N'All Types', N'جميع الأنواع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Approvals')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Approvals', N'Approvals', N'الموافقات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Approved')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Approved', N'Approved', N'معتمد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ApprovedUsers')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ApprovedUsers', N'Approved Users', N'مستخدمين معتمدين');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Apps')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Apps', N'Apps', N'التطبيقات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Arabic')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Arabic', N'Arabic', N'العربية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ArabicTranslation')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ArabicTranslation', N'Arabic Translation', N'الترجمة العربية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ArabicTranslations')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ArabicTranslations', N'Arabic Translations', N'ترجمات عربية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AreThereAdditionalMembers')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AreThereAdditionalMembers', N'Are There Additional Members', N'هل يوجد أعضاء إضافيين');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AssociatedMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AssociatedMeetings', N'Associated Meetings', N'الاجتماعات المرتبطة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Attachment')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Attachment', N'Attachment', N'المرفق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Attachments')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Attachments', N'Attachments', N'المرفقات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AttendanceRate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AttendanceRate', N'Attendance Rate', N'نسبة الحضور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AttendanceReport')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AttendanceReport', N'Attendance Report', N'تقرير الحضور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AttendanceReportDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AttendanceReportDesc', N'Attendance Report Desc', N'متابعة حضور الأعضاء في الاجتماعات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Attended')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Attended', N'Attended', N'حضر');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Attendees')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Attendees', N'Attendees', N'حضور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AttendeesStatus')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AttendeesStatus', N'Attendees Status', N'حالة الحضور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AttendeesWillBeNotified')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AttendeesWillBeNotified', N'Attendees Will Be Notified', N'سيتم إشعار الحضور تلقائياً');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AuditTrail')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AuditTrail', N'Audit Trail', N'سجل التدقيق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AutoCreateOnApproval')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AutoCreateOnApproval', N'Auto Create On Approval', N'إنشاء تلقائي عند الموافقة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AutoCreateOnApprovalDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AutoCreateOnApprovalDesc', N'Auto Create On Approval Desc', N'إنشاء اجتماع Teams تلقائياً عند الموافقة على الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AutoGenerated')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AutoGenerated', N'Auto Generated', N'يتم توليده تلقائياً');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AutoSendOptions')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AutoSendOptions', N'Auto Send Options', N'خيارات الإرسال التلقائي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AverageProgress')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AverageProgress', N'Average Progress', N'متوسط التقدم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'AzureAdSettings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'AzureAdSettings', N'Azure Ad Settings', N'إعدادات Azure Active Directory');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Back')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Back', N'Back', N'رجوع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'BackToHome')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'BackToHome', N'Back To Home', N'الرئيسية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'BasicInfo')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'BasicInfo', N'Basic Info', N'المعلومات الأساسية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Branch')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Branch', N'Branch', N'الفرع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Calendar')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Calendar', N'Calendar', N'Calendar');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Cancel')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Cancel', N'Cancel', N'إلغاء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Canceled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Canceled', N'Canceled', N'ملغية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CannotDeleteSystemTemplate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CannotDeleteSystemTemplate', N'Cannot Delete System Template', N'لا يمكن حذف قالب النظام');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CannotPreview')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CannotPreview', N'Cannot Preview', N'لا يمكن عرض هذا النوع من الملفات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CardView')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CardView', N'Card View', N'عرض البطاقات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Categories')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Categories', N'Categories', N'الفئات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Change')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Change', N'Change', N'تغيير');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ChangePassword')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ChangePassword', N'Change Password', N'تغيير كلمة المرور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ChangePasswordHint')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ChangePasswordHint', N'Change Password Hint', N'قم بتغيير كلمة المرور بشكل دوري للحفاظ على أمان حسابك');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Chat')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Chat', N'Chat', N'المحادثة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Classification')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Classification', N'Classification', N'التصنيف');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ClearSearch')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ClearSearch', N'Clear Search', N'مسح البحث');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ClickOrDragFile')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ClickOrDragFile', N'Click Or Drag File', N'اضغط أو اسحب الملف هنا');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ClientId')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ClientId', N'Client Id', N'معرف التطبيق (Client ID)');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ClientIdDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ClientIdDesc', N'Client Id Desc', N'معرف التطبيق (العميل) من تسجيل التطبيق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ClientSecret')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ClientSecret', N'Client Secret', N'سر التطبيق (Client Secret)');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ClientSecretDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ClientSecretDesc', N'Client Secret Desc', N'سر العميل من تسجيل التطبيق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Close')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Close', N'Close', N'إغلاق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Code')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Code', N'Code', N'الرمز');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CommentId')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CommentId', N'Comment Id', N'معرف التعليق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Committee')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Committee', N'Committee', N'اللجنة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CommitteeAdmin')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CommitteeAdmin', N'Committee Admin', N'مدير اللجان');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CommitteeClassification')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CommitteeClassification', N'Committee Classification', N'تصنيف اللجنة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CommitteeCouncil')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CommitteeCouncil', N'Committee Council', N'المجلس / اللجنة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CommitteeDuty')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CommitteeDuty', N'Committee Duty', N'اختصاص اللجنة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CommitteeMeeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CommitteeMeeting', N'Committee Meeting', N'مرتبط بلجنة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CommitteeName')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CommitteeName', N'Committee Name', N'اسم اللجنة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CommitteeStatus')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CommitteeStatus', N'Committee Status', N'حالة اللجنة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CommitteeStyle')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CommitteeStyle', N'Committee Style', N'نمط اللجنة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CommitteeTasks')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CommitteeTasks', N'Committee Tasks', N'مهام اللجنة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Committees')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Committees', N'Committees', N'لجان');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CommitteesSummaryDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CommitteesSummaryDesc', N'Committees Summary Desc', N'عرض ملخص شامل لجميع اللجان وإحصائياتها');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CommitteesSummaryReport')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CommitteesSummaryReport', N'Committees Summary Report', N'تقرير ملخص اللجان');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Completed')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Completed', N'Completed', N'مكتمل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CompletedOf')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CompletedOf', N'Completed Of', N'مكتملة من');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CompletionRate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CompletionRate', N'Completion Rate', N'معدل الإنجاز');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConfirmApproveBtn')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConfirmApproveBtn', N'Confirm Approve Btn', N'اعتماد مباشر');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConfirmDelete')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConfirmDelete', N'Confirm Delete', N'هل أنت متأكد من الحذف؟');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConfirmDeleteActivity')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConfirmDeleteActivity', N'Confirm Delete Activity', N'هل أنت متأكد من حذف هذا النشاط؟');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConfirmDeleteAttachment')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConfirmDeleteAttachment', N'Confirm Delete Attachment', N'هل أنت متأكد من حذف هذا المرفق؟');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConfirmDeleteMember')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConfirmDeleteMember', N'Confirm Delete Member', N'هل أنت متأكد من حذف هذا العضو؟');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConfirmDeleteOption')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConfirmDeleteOption', N'Confirm Delete Option', N'هل أنت متأكد من حذف هذا الخيار؟');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConfirmDeleteTask')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConfirmDeleteTask', N'Confirm Delete Task', N'هل أنت متأكد من حذف هذه المهمة؟');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConfirmDeleteVotingType')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConfirmDeleteVotingType', N'Confirm Delete Voting Type', N'هل أنت متأكد من حذف نوع التصويت هذا؟');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConfirmDirectApproval')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConfirmDirectApproval', N'Confirm Direct Approval', N'تأكيد الاعتماد المباشر');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConfirmDirectApprovalDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConfirmDirectApprovalDescription', N'Confirm Direct Approval Description', N'سيتم اعتماد الاجتماع مباشرة بدون الحاجة لموافقة الحضور. الاجتماع سيظهر مباشرة في التقويم.');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConfirmPassword')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConfirmPassword', N'Confirm Password', N'تأكيد كلمة المرور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConfirmSend')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConfirmSend', N'Confirm Send', N'تأكيد الإرسال');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConfirmSendBtn')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConfirmSendBtn', N'Confirm Send Btn', N'إرسال للموافقة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConfirmSendDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConfirmSendDescription', N'Confirm Send Description', N'سيتم إرسال الاجتماع للحضور للموافقة عليه. هل أنت متأكد من المتابعة؟');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConnectionFailed')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConnectionFailed', N'Connection Failed', N'فشل الاتصال');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ConnectionSuccessful')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ConnectionSuccessful', N'Connection Successful', N'الاتصال ناجح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ContactAdmin')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ContactAdmin', N'Contact Admin', N'تواصل مع المسؤول');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ControllerName')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ControllerName', N'Controller Name', N'اسم المتحكم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Council')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Council', N'Council', N'مجلس');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CouncilSession')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CouncilSession', N'Council Session', N'الدورة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Councils')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Councils', N'Councils', N'المجالس');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CouncilsAndCommittees')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CouncilsAndCommittees', N'Councils And Committees', N'المجالس واللجان');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CouncilsAndCommitteesData')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CouncilsAndCommitteesData', N'Councils And Committees Data', N'بيانات المجالس واللجان');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CreateAnother')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CreateAnother', N'Create Another', N'إنشاء اجتماع آخر');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CreateFirstMeeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CreateFirstMeeting', N'Create First Meeting', N'إنشاء أول اجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CreateSession')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CreateSession', N'Create Session', N'إنشاء الجلسة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CreatedAt')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CreatedAt', N'Created At', N'تاريخ الإنشاء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CreatedBy')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CreatedBy', N'Created By', N'أنشئت بواسطة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CreatedDate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CreatedDate', N'Created Date', N'تاريخ الإنشاء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Creator')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Creator', N'Creator', N'المنشئ');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CurrentPassword')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CurrentPassword', N'Current Password', N'كلمة المرور الحالية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'CurrentProgress')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'CurrentProgress', N'Current Progress', N'التقدم الحالي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Daily')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Daily', N'Daily', N'Daily');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Dashboard')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Dashboard', N'Dashboard', N'لوحة المعلومات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DashboardOverview')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DashboardOverview', N'Overview of your meeting management system', N'Overview of your meeting management system');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DataSources')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DataSources', N'Data Sources', N'مصادر البيانات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Date')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Date', N'Date', N'التاريخ');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Days')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Days', N'Days', N'أيام');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DbName')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DbName', N'Db Name', N'اسم قاعدة البيانات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Default')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Default', N'Default', N'افتراضي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DefaultLanguage')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DefaultLanguage', N'Default Language', N'اللغة الافتراضية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DefaultPermissions')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DefaultPermissions', N'Default Permissions', N'صلاحيات افتراضية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Delete')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Delete', N'Delete', N'حذف');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DeleteAttachment')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DeleteAttachment', N'Delete Attachment', N'حذف المرفق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DeleteOption')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DeleteOption', N'Delete Option', N'حذف الخيار');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DeleteSuccess')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DeleteSuccess', N'Delete Success', N'تم الحذف بنجاح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DeleteTemplate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DeleteTemplate', N'Delete Template', N'حذف القالب');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DeleteTemplateConfirm')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DeleteTemplateConfirm', N'Delete Template Confirm', N'هل أنت متأكد من حذف هذا القالب؟');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DeleteVotingType')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DeleteVotingType', N'Delete Voting Type', N'حذف نوع التصويت');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Description')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Description', N'Description', N'الوصف');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DidntReceiveCode')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DidntReceiveCode', N'Didnt Receive Code', N'لم تستلم الرمز؟');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DirectApprovalWarning')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DirectApprovalWarning', N'Direct Approval Warning', N'هذا الإجراء لا يمكن التراجع عنه');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Disabled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Disabled', N'Disabled', N'معطل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DisplayOrder')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DisplayOrder', N'Display Order', N'ترتيب العرض');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Document')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Document', N'Document', N'مستند');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Done')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Done', N'Done', N'تم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Download')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Download', N'Download', N'تحميل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Draft')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Draft', N'Draft', N'Draft');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DraftMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DraftMeetings', N'Draft Meetings', N'مسودات الاجتماعات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DraftMeetingsDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DraftMeetingsDescription', N'Draft Meetings Description', N'إدارة الاجتماعات المحفوظة كمسودة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DragAndDropActivities')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DragAndDropActivities', N'Drag And Drop Activities', N'قم بسحب وإفلات الأنشطة لبناء سير العمل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DragDropFiles')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DragDropFiles', N'Drag Drop Files', N'اسحب الملفات هنا أو انقر للاختيار');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DragDropOrClick')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DragDropOrClick', N'Drag Drop Or Click', N'اسحب الملف هنا أو انقر للاختيار');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DragFileOrClick')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DragFileOrClick', N'Drag File Or Click', N'اسحب الملف هنا أو انقر للاختيار');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DropFileHere')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DropFileHere', N'Drop File Here', N'اسحب الملف هنا أو انقر للاختيار');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DueDate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DueDate', N'Due Date', N'تاريخ الاستحقاق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DueIn')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DueIn', N'Due In', N'مستحق خلال');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DueSoon')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DueSoon', N'Due Soon', N'قريبة الاستحقاق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DueToday')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DueToday', N'Due Today', N'مستحق اليوم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'DueTomorrow')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'DueTomorrow', N'Due Tomorrow', N'مستحق غداً');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Duplicate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Duplicate', N'Duplicate', N'نسخ');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Duration')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Duration', N'Duration', N'المدة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Edit')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Edit', N'Edit', N'تعديل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EditActivity')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EditActivity', N'Edit Activity', N'تعديل نشاط');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EditAgendaItem')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EditAgendaItem', N'Edit Agenda Item', N'تعديل بند');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EditDepartment')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EditDepartment', N'Edit Department', N'تعديل القسم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EditEntry')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EditEntry', N'Edit Entry', N'تعديل مصطلح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EditItem')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EditItem', N'Edit Item', N'تعديل بند');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EditMember')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EditMember', N'Edit Member', N'تعديل عضو');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EditNote')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EditNote', N'Edit Note', N'تعديل الملاحظة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EditPermission')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EditPermission', N'Edit Permission', N'تعديل الصلاحية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EditRole')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EditRole', N'Edit Role', N'تعديل الدور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EditTask')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EditTask', N'Edit Task', N'تعديل مهمة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EditTemplate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EditTemplate', N'Edit Template', N'تعديل القالب');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EditUser')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EditUser', N'Edit User', N'تعديل مستخدم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EditVotingType')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EditVotingType', N'Edit Voting Type', N'تعديل نوع التصويت');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Email')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Email', N'Email', N'البريد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnableEmail')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnableEmail', N'Enable Email', N'البريد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnableSSL')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnableSSL', N'Enable SSL', N'تفعيل SSL/TLS');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnableSms')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnableSms', N'Enable Sms', N'الرسائل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Enabled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Enabled', N'Enabled', N'مفعل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EndDate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EndDate', N'End Date', N'تاريخ النهاية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EndMeeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EndMeeting', N'End Meeting', N'إنهاء الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EndTime')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EndTime', N'End Time', N'وقت الانتهاء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'English')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'English', N'English', N'الإنجليزية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnglishTranslation')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnglishTranslation', N'English Translation', N'الترجمة الإنجليزية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnglishTranslations')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnglishTranslations', N'English Translations', N'ترجمات إنجليزية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnjoyYourDay')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnjoyYourDay', N'Enjoy Your Day', N'استمتع بيومك!');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterArabic')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterArabic', N'Enter Arabic', N'أدخل النص بالعربية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterDescription', N'Enter Description', N'أدخل وصف الصلاحية...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterDuration')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterDuration', N'Enter Duration', N'أدخل المدة بالدقائق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterEmail')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterEmail', N'Enter Email', N'أدخل البريد الإلكتروني');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterEnglish')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterEnglish', N'Enter text in English', N'Enter text in English');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterExternalId')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterExternalId', N'Enter External Id', N'أدخل المعرف الخارجي (اختياري)');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterExternalReferenceNumber')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterExternalReferenceNumber', N'Enter External Reference Number', N'أدخل الرقم المرجعي الخارجي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterFullNameAr')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterFullNameAr', N'Enter Full Name Ar', N'أدخل الاسم الكامل بالعربية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterFullNameEn')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterFullNameEn', N'Enter full name in English', N'Enter full name in English');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterHtmlTemplate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterHtmlTemplate', N'Enter Html Template', N'أدخل قالب HTML هنا...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterInternalNote')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterInternalNote', N'Enter Internal Note', N'أدخل ملاحظة داخلية (اختياري)');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterItemSubject')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterItemSubject', N'Enter Item Subject', N'أدخل موضوع البند');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterKeyword')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterKeyword', N'Enter Keyword', N'أدخل الكلمة المفتاحية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterLocation')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterLocation', N'Enter Location', N'أدخل مكان الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterMeetingSubject')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterMeetingSubject', N'Enter Meeting Subject', N'أدخل موضوع الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterMeetingUrl')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterMeetingUrl', N'https://meet.example.com/meeting-id', N'https://meet.example.com/meeting-id');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterMobile')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterMobile', N'Enter Mobile', N'أدخل رقم الجوال');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterNameAr')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterNameAr', N'Enter Name Ar', N'أدخل اسم الدور بالعربية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterNameEn')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterNameEn', N'Enter role name in English', N'Enter role name in English');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterNationalId')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterNationalId', N'Enter National Id', N'أدخل رقم الهوية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterNewPassword')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterNewPassword', N'Enter New Password', N'أدخل كلمة المرور الجديدة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterNote')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterNote', N'Enter Note', N'أدخل الملاحظة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterNotesPlaceholder')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterNotesPlaceholder', N'Enter Notes Placeholder', N'أضف ملاحظات أو تعليمات خاصة للحضور...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterNumber')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterNumber', N'Enter Number', N'أدخل الرقم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterReferenceNumber')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterReferenceNumber', N'Enter Reference Number', N'أدخل الرقم المرجعي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterSubject')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterSubject', N'Enter Subject', N'أدخل موضوع البند');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterTags')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterTags', N'Enter Tags', N'اكتب وسم واضغط Enter');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterTitle')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterTitle', N'Enter Title', N'أدخل عنوانا');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterTopic')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterTopic', N'Enter Topic', N'أدخل المحور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterUsername')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterUsername', N'Enter Username', N'أدخل اسم المستخدم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'EnterValue')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'EnterValue', N'Enter Value', N'أدخل القيمة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Entries')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Entries', N'Entries', N'سجل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ErrorLoadingData')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ErrorLoadingData', N'Error Loading Data', N'حدث خطأ أثناء تحميل البيانات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ErrorLoadingFile')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ErrorLoadingFile', N'Error Loading File', N'خطأ في تحميل الملف');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ErrorLoadingPermissions')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ErrorLoadingPermissions', N'Error Loading Permissions', N'حدث خطأ أثناء تحميل الصلاحيات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ErrorOccurred')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ErrorOccurred', N'Error Occurred', N'حدث خطأ');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ErrorSavingData')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ErrorSavingData', N'Error Saving Data', N'حدث خطأ أثناء حفظ البيانات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ErrorUpdatingPermission')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ErrorUpdatingPermission', N'Error Updating Permission', N'حدث خطأ أثناء تحديث الصلاحية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ExpectCalendarEvent')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ExpectCalendarEvent', N'Expect Calendar Event', N'سيظهر الحدث في تقويمهم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ExpectEmail')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ExpectEmail', N'Expect Email', N'سيتلقى الحضور بريداً إلكترونياً مع مرفق .ics');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ExpectOutlookButtons')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ExpectOutlookButtons', N'Expect Outlook Buttons', N'في Outlook، سيرون أزرار قبول/رفض/مبدئي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ExternalId')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ExternalId', N'External Id', N'المعرف الخارجي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ExternalReferenceNumber')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ExternalReferenceNumber', N'External Reference Number', N'الرقم المرجعي الخارجي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'FarthestFirst')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'FarthestFirst', N'Farthest First', N'Farthest First');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'FeatureComingSoon')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'FeatureComingSoon', N'Feature Coming Soon', N'هذه الميزة قيد التطوير');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'FileIsNotSupported')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'FileIsNotSupported', N'File Is Not Supported', N'هذا النوع من الملفات غير مدعوم للعرض');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'FilePlaceHolder')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'FilePlaceHolder', N'File Place Holder', N'اختر ملفا لعرضه');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Files')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Files', N'Files', N'ملف');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Filters')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Filters', N'Filters', N'التصفية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'FinalMOM')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'FinalMOM', N'Final MOM', N'محضر نهائي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'FinalMinutes')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'FinalMinutes', N'Final Minutes', N'المحضر النهائي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'FinancialCompensation')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'FinancialCompensation', N'Financial Compensation', N'تعويض مالي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Finished')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Finished', N'Finished', N'انتهى');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'FormBuilder')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'FormBuilder', N'Form Builder', N'بناء النموذج');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'FormBuilderConfig')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'FormBuilderConfig', N'Form Builder Config', N'أداة بناء النماذج');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'FromDate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'FromDate', N'From Date', N'من تاريخ');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'FullName')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'FullName', N'Full Name', N'الاسم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'FullNameAr')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'FullNameAr', N'Full Name Ar', N'الاسم بالعربي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'FullNameEn')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'FullNameEn', N'Full Name En', N'الاسم بالإنجليزي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'General')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'General', N'General', N'عام');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'GoBack')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'GoBack', N'Go Back', N'العودة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'GoHome')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'GoHome', N'Go Home', N'الرئيسية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'GoodAfternoon')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'GoodAfternoon', N'Good Afternoon', N'مساء الخير');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'GoodEvening')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'GoodEvening', N'Good Evening', N'مساء الخير');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'GoodMorning')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'GoodMorning', N'Good Morning', N'صباح الخير');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'GraphApiSettings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'GraphApiSettings', N'Graph Api Settings', N'إعدادات Microsoft Graph API');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'GraphModeDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'GraphModeDesc', N'Graph Mode Desc', N'تكامل مباشر مع Microsoft 365');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'HomeDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'HomeDescription', N'Home Description', N'لوحة التحكم الرئيسية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'HoursMinutesSeconds')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'HoursMinutesSeconds', N'Hours Minutes Seconds', N'ساعات دقائق ثواني');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'HtmlTemplate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'HtmlTemplate', N'Html Template', N'قالب HTML');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'HtmlTemplateDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'HtmlTemplateDescription', N'Html Template Description', N'يمكنك استخدام المتغيرات التالية في القالب:');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Icon')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Icon', N'Icon', N'الأيقونة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'InProgress')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'InProgress', N'In Progress', N'قيد التنفيذ');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Inactive')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Inactive', N'Inactive', N'غير نشط');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'InactiveUsers')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'InactiveUsers', N'Inactive Users', N'المستخدمين غير النشطين');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'InitialMOM')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'InitialMOM', N'Initial MOM', N'محضر أولي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'InitialMinutes')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'InitialMinutes', N'Initial Minutes', N'المحضر المبدئي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'InitiatedWorkflows')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'InitiatedWorkflows', N'Initiated Workflows', N'سير العمل المبدوء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'InstanceName')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'InstanceName', N'Instance Name', N'اسم الخادم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'IntegrationDisabled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'IntegrationDisabled', N'Integration Disabled', N'التكامل معطل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'IntegrationEnabled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'IntegrationEnabled', N'Integration Enabled', N'التكامل مفعل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'IntegrationMode')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'IntegrationMode', N'Integration Mode', N'وضع التكامل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'IntegrationSettings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'IntegrationSettings', N'Integration Settings', N'إعدادات التكامل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'InternalNote')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'InternalNote', N'Internal Note', N'ملاحظة داخلية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'InvalidAttachmentParams')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'InvalidAttachmentParams', N'Invalid Attachment Params', N'معاملات المرفق غير صالحة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'IsDefaultPermission')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'IsDefaultPermission', N'Is Default Permission', N'صلاحية افتراضية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'IsDocumentMapping')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'IsDocumentMapping', N'Is Document Mapping', N'ربط المستندات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'IsInternal')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'IsInternal', N'Is Internal', N'داخلي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'IsPresentationRelated')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'IsPresentationRelated', N'Is Presentation Related', N'ذات علاقة بالعرض');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'IsPrimary')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'IsPrimary', N'Is Primary', N'أساسي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ItemType')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ItemType', N'Item Type', N'النوع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Items')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Items', N'Items', N'بند');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Keyword')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Keyword', N'Keyword', N'الكلمة المفتاحية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'KeywordHint')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'KeywordHint', N'Keyword Hint', N'المعرف الفريد للمصطلح في النظام');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Language')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Language', N'Language', N'اللغة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'LatestUpdates')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'LatestUpdates', N'Latest Updates', N'آخر التحديثات والأحداث');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'LdapEnabled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'LdapEnabled', N'LDAP', N'LDAP');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'LdapIsDisabled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'LdapIsDisabled', N'Ldap Is Disabled', N'تم تعطيل LDAP');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'LdapIsEnabled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'LdapIsEnabled', N'Ldap Is Enabled', N'تم تفعيل LDAP');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'LetterId')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'LetterId', N'Letter Id', N'معرف الخطاب');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'LinkedMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'LinkedMeetings', N'Linked Meetings', N'الاجتماعات المرتبطة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'List')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'List', N'List', N'List');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Live')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Live', N'Live', N'الآن');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Loading')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Loading', N'Loading', N'جاري التحميل...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'LoadingAttachment')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'LoadingAttachment', N'Loading Attachment', N'جاري تحميل المرفق...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'LoadingDrafts')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'LoadingDrafts', N'Loading Drafts', N'جاري تحميل المسودات...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'LoadingFile')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'LoadingFile', N'Loading File', N'جاري تحميل الملف...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'LoadingMeeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'LoadingMeeting', N'Loading Meeting', N'جاري تحميل بيانات الاجتماع...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'LoadingMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'LoadingMeetings', N'Loading Meetings', N'جاري تحميل الاجتماعات...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'LoadingPermissions')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'LoadingPermissions', N'Loading Permissions', N'جاري تحميل الصلاحيات...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'LoadingProcess')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'LoadingProcess', N'Loading Process', N'جاري تحميل سير العمل...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Location')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Location', N'Location', N'المكان');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Logout')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Logout', N'Logout', N'تسجيل الخروج');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'LowercaseLetter')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'LowercaseLetter', N'Lowercase Letter', N'حرف صغير واحد (a-z)');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ManageAndTrackRecommendations')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ManageAndTrackRecommendations', N'Manage And Track Recommendations', N'إدارة ومتابعة التوصيات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ManageCouncilsAndCommittees')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ManageCouncilsAndCommittees', N'Manage Councils And Committees', N'إدارة المجالس واللجان والاطلاع على تفاصيلها');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ManageOrganization')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ManageOrganization', N'Manage Organization', N'إدارة المنظمة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ManagePermissionsFor')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ManagePermissionsFor', N'Manage Permissions For', N'إدارة الصلاحيات لـ');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ManagePrivileges')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ManagePrivileges', N'Manage Privileges', N'إدارة الصلاحيات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ManageProfileInfo')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ManageProfileInfo', N'Manage Profile Info', N'إدارة معلومات حسابك الشخصية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Meeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Meeting', N'Meeting', N'اجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingAttachments')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingAttachments', N'Meeting Attachments', N'مرفقات الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingDate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingDate', N'Meeting Date', N'تاريخ الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingDetails')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingDetails', N'Meeting Details', N'تفاصيل الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingDirectApproval')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingDirectApproval', N'Meeting Direct Approval', N'اعتماد مباشر');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingInfo')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingInfo', N'Meeting Info', N'معلومات الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingNumber')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingNumber', N'Meeting Number', N'رقم الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingReference')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingReference', N'Meeting Reference', N'مرجع الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingRoom')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingRoom', N'Meeting Room', N'قاعة الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingSubject')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingSubject', N'Meeting Subject', N'موضوع الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingSubmitted')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingSubmitted', N'Meeting Submitted', N'تم إرسال الاجتماع بنجاح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingSubmittedDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingSubmittedDescription', N'Meeting Submitted Description', N'تم إرسال الاجتماع للحضور للمراجعة والموافقة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingType')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingType', N'Meeting Type', N'نوع الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingUrl')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingUrl', N'Meeting Url', N'رابط الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Meetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Meetings', N'Meetings', N'الاجتماعات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingsByStatus')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingsByStatus', N'Meetings By Status', N'الاجتماعات حسب الحالة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingsCalendar')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingsCalendar', N'Meetings Calendar', N'تقويم الاجتماعات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingsCount')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingsCount', N'Meetings Count', N'عدد الاجتماعات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingsDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingsDescription', N'Meetings Description', N'إدارة وعرض جميع الاجتماعات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingsDetails')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingsDetails', N'Meetings Details', N'تفاصيل الاجتماعات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingsNotLinkedToCommittee')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingsNotLinkedToCommittee', N'Meetings Not Linked To Committee', N'اجتماعات غير المرتبطة بأي لجنة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MeetingsOverview')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MeetingsOverview', N'Meetings Overview', N'نظرة عامة على حالات الاجتماعات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Member')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Member', N'Member', N'العضو');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Members')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Members', N'Members', N'الأعضاء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MembersCount')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MembersCount', N'Members Count', N'عدد الأعضاء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Messages')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Messages', N'Messages', N'الرسائل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Min8Characters')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Min8Characters', N'Min8Characters', N'8 أحرف على الأقل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Minutes')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Minutes', N'Minutes', N'دقيقة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'MinutesApproved')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'MinutesApproved', N'Minutes Approved', N'Minutes Approved');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Mobile')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Mobile', N'Mobile', N'رقم الجوال');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Monthly')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Monthly', N'Monthly', N'Monthly');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Name')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Name', N'Name', N'الاسم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NameAr')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NameAr', N'Name Ar', N'الاسم بالعربية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NameEn')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NameEn', N'Name En', N'الاسم بالإنجليزية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NationalId')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NationalId', N'National Id', N'رقم الهوية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NearestFirst')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NearestFirst', N'Nearest First', N'Nearest First');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NeedsAttention')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NeedsAttention', N'Needs Attention', N'تحتاج انتباهك');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NewMeeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NewMeeting', N'New Meeting', N'اجتماع جديد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NewPassword')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NewPassword', N'New Password', N'كلمة المرور الجديدة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NewProcess')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NewProcess', N'New Process', N'عملية جديدة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Next')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Next', N'Next', N'التالي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NextScheduled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NextScheduled', N'Next Scheduled', N'المواعيد المجدولة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoActivities')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoActivities', N'No Activities', N'لا توجد أنشطة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoAdditionalInfo')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoAdditionalInfo', N'No Additional Info', N'لا توجد معلومات إضافية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoAgendaItems')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoAgendaItems', N'No Agenda Items', N'لم يتم إضافة بنود بعد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoApprovalsRequired')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoApprovalsRequired', N'No Approvals Required', N'لا توجد موافقات مطلوبة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoAppsFound')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoAppsFound', N'No Apps Found', N'لا توجد تطبيقات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoAttachments')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoAttachments', N'No Attachments', N'لا توجد مرفقات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoAttendees')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoAttendees', N'No Attendees', N'لم يتم إضافة حضور بعد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoChats')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoChats', N'No Chats', N'لا توجد محادثات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoClassification')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoClassification', N'No Classification', N'-- بدون تصنيف --');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoCouncilsOrCommitteesFound')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoCouncilsOrCommitteesFound', N'No Councils Or Committees Found', N'لم يتم العثور على مجالس أو لجان');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoData')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoData', N'No Data', N'لا توجد بيانات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoDepartments')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoDepartments', N'No Departments', N'لا توجد أقسام');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoDraftsDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoDraftsDescription', N'No Drafts Description', N'ابدأ بإنشاء اجتماع جديد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoDraftsFound')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoDraftsFound', N'No Drafts Found', N'لا توجد مسودات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoItemsToShow')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoItemsToShow', N'No Items To Show', N'اختر هيكل لعرض التفاصيل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoLinkedMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoLinkedMeetings', N'No Linked Meetings', N'لا توجد اجتماعات مرتبطة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoLocation')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoLocation', N'No Location', N'غير محدد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoMatchingPermissions')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoMatchingPermissions', N'No Matching Permissions', N'لا توجد صلاحيات مطابقة للبحث');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoMeetings', N'No Meetings', N'لا توجد اجتماعات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoMeetingsDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoMeetingsDescription', N'No Meetings Description', N'لم يتم العثور على اجتماعات تطابق معايير البحث');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoMeetingsFound')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoMeetingsFound', N'No Meetings Found', N'لا توجد نتائج');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoMeetingsMatchFilter')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoMeetingsMatchFilter', N'No meetings match the selected filter', N'No meetings match the selected filter');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoMeetingsToday')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoMeetingsToday', N'No Meetings Today', N'لا توجد اجتماعات مجدولة اليوم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoMembers')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoMembers', N'No Members', N'لا يوجد أعضاء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoMessages')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoMessages', N'No Messages', N'لا توجد رسائل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoNotes')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoNotes', N'No Notes', N'لا توجد ملاحظات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoOptions')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoOptions', N'No Options', N'لا توجد خيارات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoParent')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoParent', N'No Parent', N'-- بدون قسم أب --');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoPermissions')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoPermissions', N'No Permissions', N'لا توجد صلاحيات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoPermissionsDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoPermissionsDescription', N'No Permissions Description', N'لم يتم تعيين صلاحيات لهذا المستخدم بعد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoPermissionsFound')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoPermissionsFound', N'No Permissions Found', N'لا توجد صلاحيات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoRecentActivity')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoRecentActivity', N'No Recent Activity', N'لا يوجد نشاط حديث');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoRecommendations')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoRecommendations', N'No Recommendations', N'لا توجد توصيات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoRecommendationsFound')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoRecommendationsFound', N'No Recommendations Found', N'لا توجد توصيات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoRelatedItem')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoRelatedItem', N'No Related Item', N'لا يوجد بند مرتبط');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoResults')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoResults', N'No Results', N'لا توجد نتائج');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoRoles')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoRoles', N'No Roles', N'لا توجد أدوار');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoRolesAssigned')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoRolesAssigned', N'No Roles Assigned', N'لا توجد أدوار مسندة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoSessionItems')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoSessionItems', N'No Session Items', N'لا توجد بنود');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoSettingsFound')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoSettingsFound', N'No Settings Found', N'لم يتم العثور على إعدادات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoTasks')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoTasks', N'No Tasks', N'لا توجد مهام');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoTemplatesFound')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoTemplatesFound', N'No Templates Found', N'لا توجد قوالب');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoUpcomingMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoUpcomingMeetings', N'No Upcoming Meetings', N'لا توجد اجتماعات قادمة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoUsers')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoUsers', N'No Users', N'لا يوجد مستخدمين');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoUsersInDepartment')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoUsersInDepartment', N'No Users In Department', N'لا يوجد مستخدمين في هذا القسم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoVoting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoVoting', N'No Voting', N'بدون تصويت');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoVotingTypes')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoVotingTypes', N'No Voting Types', N'لا توجد أنواع تصويت');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NotRelatedMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NotRelatedMeetings', N'Not Related Meetings', N'الاجتماعات غير المرتبطة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Note')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Note', N'Note', N'ملاحظة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'NoteText')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'NoteText', N'Note Text', N'نص الملاحظة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Notes')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Notes', N'Notes', N'ملاحظات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Offline')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Offline', N'Offline', N'غير متصل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OneDigit')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OneDigit', N'One Digit', N'رقم واحد (0-9)');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Online')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Online', N'Online', N'متصل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OnlineMeetingsReadWriteAll')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OnlineMeetingsReadWriteAll', N'Online Meetings Read Write All', N'لإنشاء اجتماعات Teams');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OnlyMyMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OnlyMyMeetings', N'Only My Meetings', N'اجتماعاتي فقط');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Open')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Open', N'Open', N'فتح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OpenMeeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OpenMeeting', N'Open Meeting', N'فتح الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OpenMeetingDashboard')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OpenMeetingDashboard', N'Open Meeting Dashboard', N'فتح لوحة الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OpenNewWindow')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OpenNewWindow', N'Open New Window', N'فتح في نافذة جديدة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OptionNameAr')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OptionNameAr', N'Option Name Ar', N'اسم الخيار بالعربية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OptionNameEn')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OptionNameEn', N'Option name in English', N'Option name in English');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Optional')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Optional', N'Optional', N'اختياري');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Options')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Options', N'Options', N'خيارات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Order')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Order', N'Order', N'الترتيب');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OrganizationStructure')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OrganizationStructure', N'Organization Structure', N'الهيكل التنظيمي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OrganizerEmail')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OrganizerEmail', N'Organizer Email', N'البريد الإلكتروني للمنظم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OrganizerEmailDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OrganizerEmailDesc', N'Organizer Email Desc', N'يجب أن يكون مستخدم مرخص في Microsoft 365 لإنشاء اجتماعات Teams');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OrganizerSettings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OrganizerSettings', N'Organizer Settings', N'إعدادات المنظم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OutlookIntegrationDisabledDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OutlookIntegrationDisabledDesc', N'Outlook Integration Disabled Desc', N'لن يتم إرسال دعوات التقويم. قم بتفعيل التكامل للبدء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OutlookIntegrationEnabledDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OutlookIntegrationEnabledDesc', N'Outlook Integration Enabled Desc', N'سيتم إرسال دعوات التقويم تلقائياً عند الموافقة على الاجتماعات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Overdue')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Overdue', N'Overdue', N'متأخرة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OverdueBy')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OverdueBy', N'Overdue By', N'متأخر');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OverdueByOneDay')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OverdueByOneDay', N'Overdue By One Day', N'متأخر يوم واحد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'OverdueTasks')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'OverdueTasks', N'Overdue Tasks', N'متأخرة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Owner')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Owner', N'Owner', N'المسؤول');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Page')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Page', N'Page', N'صفحة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PageNotFound')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PageNotFound', N'Page Not Found', N'الصفحة غير موجودة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PageNotFoundMessage')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PageNotFoundMessage', N'Page Not Found Message', N'عذراً، الصفحة التي تبحث عنها غير موجودة.');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ParentActivityNumber')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ParentActivityNumber', N'Parent Activity Number', N'رقم النشاط الأب');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ParentCouncilOrCommittee')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ParentCouncilOrCommittee', N'Parent Council Or Committee', N'المجلس أو اللجنة الأم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ParentDepartment')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ParentDepartment', N'Parent Department', N'القسم الأب');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Password')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Password', N'Password', N'كلمة المرور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PasswordRequirements')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PasswordRequirements', N'Password Requirements', N'متطلبات كلمة المرور:');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Pending')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Pending', N'Pending', N'معلق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PendingApproval')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PendingApproval', N'Pending Approval', N'Pending Approval');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PendingApprovals')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PendingApprovals', N'Pending Approvals', N'بانتظار الموافقة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PendingMinutesApproval')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PendingMinutesApproval', N'Pending Minutes Approval', N'Pending Minutes Approval');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PendingSignature')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PendingSignature', N'Pending Signature', N'Pending Signature');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PendingTasks')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PendingTasks', N'Pending Tasks', N'مهام معلقة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PerPage')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PerPage', N'Per Page', N'لكل صفحة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PermissionTypes')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PermissionTypes', N'Permission Types', N'أنواع الصلاحيات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Permissions')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Permissions', N'Permissions', N'الصلاحيات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PermissionsAutoSave')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PermissionsAutoSave', N'Permissions Auto Save', N'يتم حفظ التغييرات تلقائياً');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PermissionsConfigurationComingSoon')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PermissionsConfigurationComingSoon', N'Permissions Configuration Coming Soon', N'إعدادات الصلاحيات قيد التطوير');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PermissionsInfo')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PermissionsInfo', N'Permissions Info', N'يجب منح التطبيق الصلاحيات التالية في Azure AD:');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PersonalMeeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PersonalMeeting', N'Personal Meeting', N'اجتماع خاص');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Present')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Present', N'Present', N'حاضر');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Previous')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Previous', N'Previous', N'السابق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PrivacyLevel')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PrivacyLevel', N'Privacy Level', N'مستوى الخصوصية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ProcessFollowup')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ProcessFollowup', N'Process Followup', N'متابعة العمليات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ProcessInstanceId')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ProcessInstanceId', N'Process Instance Id', N'معرف العملية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ProcessNumber')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ProcessNumber', N'Process Number', N'رقم العملية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ProcessTitle')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ProcessTitle', N'Process Title', N'عنوان العملية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ProcessType')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ProcessType', N'Process Type', N'نوع العملية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ProcessingLogin')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ProcessingLogin', N'Processing Login', N'جاري تسجيل الدخول...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Profile')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Profile', N'Profile', N'الملف الشخصي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ProfilePictureHint')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ProfilePictureHint', N'Profile Picture Hint', N'يُنصح بصورة مربعة لا تقل عن 200x200 بكسل. الحد الأقصى 5MB.');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Progress')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Progress', N'Progress', N'التقدم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Publish')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Publish', N'Publish', N'نشر');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PublishAsNewVersion')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PublishAsNewVersion', N'Publish As New Version', N'نشر كإصدار جديد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'PublishWorkflowDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'PublishWorkflowDescription', N'Publish Workflow Description', N'سيتم نشر سير العمل وإتاحته للاستخدام');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'QuickStats')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'QuickStats', N'Quick Stats', N'إحصائيات سريعة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ReadyToPublish')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ReadyToPublish', N'Ready To Publish', N'جاهز للنشر');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RecentActivity')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RecentActivity', N'Recent Activity', N'النشاط الأخير');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Recommendation')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Recommendation', N'Recommendation', N'توصية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RecommendationCompleted')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RecommendationCompleted', N'Recommendation Completed', N'تم إنجاز هذه التوصية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RecommendationDetails')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RecommendationDetails', N'Recommendation Details', N'تفاصيل التوصية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RecommendationNo')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RecommendationNo', N'Recommendation No', N'رقم التوصية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RecommendationText')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RecommendationText', N'Recommendation Text', N'نص التوصية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Recommendations')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Recommendations', N'Recommendations', N'التوصيات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RecommendationsCount')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RecommendationsCount', N'Recommendations Count', N'عدد التوصيات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RecommendationsProgress')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RecommendationsProgress', N'Recommendations Progress', N'تقدم التوصيات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RecordId')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RecordId', N'Record Id', N'معرف السجل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Records')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Records', N'Records', N'سجل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ReferenceNumber')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ReferenceNumber', N'Reference Number', N'الرقم المرجعي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Refresh')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Refresh', N'Refresh', N'تحديث');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RelatedAgenda')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RelatedAgenda', N'Related Agenda', N'ربط ببند');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RelatedCommittee')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RelatedCommittee', N'Related Committee', N'مرتبط بلجنة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RelatedItem')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RelatedItem', N'Related Item', N'البند المرتبط');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ReloadSettings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ReloadSettings', N'Reload Settings', N'إعادة تحميل الإعدادات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Remove')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Remove', N'Remove', N'إزالة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Required')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Required', N'Required', N'إلزامي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RequiredField')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RequiredField', N'Required Field', N'جميع الحقول مطلوبة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RequiredFieldsMissing')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RequiredFieldsMissing', N'Required Fields Missing', N'الحقول المطلوبة مفقودة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RequiredForSending')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RequiredForSending', N'Required For Sending', N'مطلوب للإرسال');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'RequiredPermissions')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'RequiredPermissions', N'Required Permissions', N'الصلاحيات المطلوبة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Reset')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Reset', N'Reset', N'إعادة تعيين');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ResetPassword')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ResetPassword', N'Reset Password', N'إعادة تعيين كلمة المرور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Retry')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Retry', N'Retry', N'إعادة المحاولة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ReturnToLogin')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ReturnToLogin', N'Return To Login', N'العودة لتسجيل الدخول');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Role')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Role', N'Role', N'الدور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Roles')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Roles', N'Roles', N'الأدوار');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Running')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Running', N'Running', N'Running');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SMS')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SMS', N'SMS', N'الرسائل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Save')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Save', N'Save', N'حفظ');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SaveAndContinue')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SaveAndContinue', N'Save And Continue', N'حفظ والمتابعة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SaveChanges')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SaveChanges', N'Save Changes', N'حفظ التغييرات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SaveDraft')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SaveDraft', N'Save Draft', N'حفظ كمسودة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SavedSuccessfully')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SavedSuccessfully', N'Saved Successfully', N'تم الحفظ بنجاح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ScheduleMeeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ScheduleMeeting', N'Schedule Meeting', N'جدولة اجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Scheduled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Scheduled', N'Scheduled', N'مجدولة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Search')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Search', N'Search', N'بحث...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchAndFilter')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchAndFilter', N'Search And Filter', N'البحث والتصفية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchByAgenda')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchByAgenda', N'Search By Agenda', N'البحث في جدول الأعمال...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchByCommittee')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchByCommittee', N'Search By Committee', N'البحث باسم اللجنة...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchByMeeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchByMeeting', N'Search By Meeting', N'البحث بموضوع الاجتماع...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchByNotes')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchByNotes', N'Search By Notes', N'البحث في الملاحظات...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchByRecommendation')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchByRecommendation', N'Search By Recommendation', N'البحث في التوصيات...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchBySubjectOrId')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchBySubjectOrId', N'Search By Subject Or Id', N'ابحث بالموضوع أو المعرف الخارجي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchByTitle')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchByTitle', N'Search By Title', N'البحث بالعنوان');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchByTopic')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchByTopic', N'Search By Topic', N'البحث في المواضيع...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchDepartments')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchDepartments', N'Search Departments', N'البحث في الأقسام...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchDictionary')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchDictionary', N'Search Dictionary', N'البحث في القاموس...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchMeetings', N'Search Meetings', N'ابحث عن اجتماع للربط...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchPermissions')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchPermissions', N'Search Permissions', N'البحث في الصلاحيات...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchRelatedItems')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchRelatedItems', N'Search Related Items', N'البحث عن بنود مرتبطة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchRoles')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchRoles', N'Search Roles', N'البحث في الأدوار...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchTemplates')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchTemplates', N'Search Templates', N'البحث في القوالب...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchTextInput')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchTextInput', N'Search Text Input', N'بحث...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchUser')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchUser', N'Search User', N'البحث عن مستخدم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchUsers')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchUsers', N'Search Users', N'البحث عن مستخدم...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SearchVotingTypes')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SearchVotingTypes', N'Search Voting Types', N'البحث في أنواع التصويت...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Searching')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Searching', N'Searching', N'جاري البحث...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Select')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Select', N'Select', N'اختر');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectBranch')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectBranch', N'Select Branch', N'-- اختر الفرع --');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectChat')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectChat', N'Select Chat', N'اختر محادثة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectChatDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectChatDescription', N'Select Chat Description', N'اختر محادثة من القائمة أو ابدأ محادثة جديدة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectCommittee')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectCommittee', N'Select Committee', N'اختر اللجنة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectDatasource')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectDatasource', N'Select Datasource', N'اختر مصدر البيانات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectDate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectDate', N'Select Date', N'اختر التاريخ');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectDepartment')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectDepartment', N'Select Department', N'اختر قسم لعرض التفاصيل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectFile')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectFile', N'Select File', N'اختر ملف');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectItem')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectItem', N'Select Item', N'اختر مجلس أو لجنة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectItemDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectItemDesc', N'Select Item Desc', N'اختر عنصر من القائمة لعرض التفاصيل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectItemType')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectItemType', N'Select Item Type', N'اختر نوع البند');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectLanguage')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectLanguage', N'Select Language', N'اختر اللغة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectMeeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectMeeting', N'Select Meeting', N'اختر اجتماع للاختبار');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectMeetingType')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectMeetingType', N'Select Meeting Type', N'اختر نوع الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectPrivacy')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectPrivacy', N'Select Privacy', N'اختر مستوى الخصوصية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectRole')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectRole', N'Select Role', N'اختر الدور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectSession')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectSession', N'Select Session', N'اختر الدورة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectTime')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectTime', N'Select Time', N'اختر الوقت');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectType')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectType', N'Select Type', N'-- اختر النوع --');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectUser')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectUser', N'Select User', N'اختر المستخدم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectUserSearch')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectUserSearch', N'Select User Search', N'ابحث عن مستخدم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SelectedMeeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SelectedMeeting', N'Selected Meeting', N'الاجتماع المحدد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Send')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Send', N'Send', N'إرسال');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SendCancellationOnCancel')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SendCancellationOnCancel', N'Send Cancellation On Cancel', N'إرسال إلغاء عند الإلغاء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SendCancellationOnCancelDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SendCancellationOnCancelDesc', N'Send Cancellation On Cancel Desc', N'إرسال إشعار إلغاء للحضور عند إلغاء الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SendInviteOnApproval')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SendInviteOnApproval', N'Send Invite On Approval', N'إرسال دعوة عند الموافقة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SendInviteOnApprovalDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SendInviteOnApprovalDesc', N'Send Invite On Approval Desc', N'إرسال دعوة تقويم تلقائياً عند الموافقة على الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SendTestInvite')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SendTestInvite', N'Send Test Invite', N'إرسال دعوة تجريبية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SendUpdateOnChange')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SendUpdateOnChange', N'Send Update On Change', N'إرسال تحديث عند التعديل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SendUpdateOnChangeDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SendUpdateOnChangeDesc', N'Send Update On Change Desc', N'إرسال تحديث للحضور عند تعديل تفاصيل الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SessionCreatedDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SessionCreatedDescription', N'Session Created Description', N'تم إنشاء الجلسة وحفظ البنود بنجاح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SessionCreatedSuccessfully')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SessionCreatedSuccessfully', N'Session Created Successfully', N'تم إنشاء الجلسة بنجاح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SessionInfo')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SessionInfo', N'Session Info', N'معلومات الجلسة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SessionItems')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SessionItems', N'Session Items', N'بنود الجلسة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SetAsDefault')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SetAsDefault', N'Set As Default', N'تعيين كافتراضي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Settings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Settings', N'Settings', N'الإعدادات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SettingsDescription')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SettingsDescription', N'Settings Description', N'إدارة إعدادات النظام والتكوينات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ShowDeactivatedStructures')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ShowDeactivatedStructures', N'Show Deactivated Structures', N'عرض الهياكل غير النشطة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ShowInactive')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ShowInactive', N'Show Inactive', N'غير النشطة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ShowInactiveUsers')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ShowInactiveUsers', N'Show Inactive Users', N'عرض المستخدمين غير النشطين');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Showing')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Showing', N'Showing', N'عرض');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Signed')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Signed', N'Signed', N'Signed');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SmsDisabled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SmsDisabled', N'Sms Disabled', N'تم تعطيل الرسائل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SmsEnabled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SmsEnabled', N'Sms Enabled', N'تم تفعيل الرسائل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SmtpFromEmail')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SmtpFromEmail', N'Smtp From Email', N'البريد المرسل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SmtpFromEmailPlaceholder')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SmtpFromEmailPlaceholder', N'noreply@domain.com', N'noreply@domain.com');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SmtpHost')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SmtpHost', N'Smtp Host', N'خادم SMTP');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SmtpHostPlaceholder')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SmtpHostPlaceholder', N'smtp.office365.com', N'smtp.office365.com');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SmtpModeDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SmtpModeDesc', N'Smtp Mode Desc', N'إرسال عبر خادم البريد SMTP');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SmtpPassword')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SmtpPassword', N'Smtp Password', N'كلمة المرور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SmtpPort')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SmtpPort', N'Smtp Port', N'المنفذ');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SmtpSettings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SmtpSettings', N'Smtp Settings', N'إعدادات SMTP');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SmtpUser')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SmtpUser', N'Smtp User', N'اسم المستخدم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SmtpUserPlaceholder')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SmtpUserPlaceholder', N'user@domain.com', N'user@domain.com');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SourceId')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SourceId', N'Source Id', N'معرف المصدر');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SpecialCharacter')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SpecialCharacter', N'Special Character', N'رمز خاص (~!@#$%^*-_=+)');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Stage')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Stage', N'Stage', N'المرحلة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'StartConversation')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'StartConversation', N'Start Conversation', N'ابدأ المحادثة الآن');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'StartDate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'StartDate', N'Start Date', N'تاريخ البداية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'StartMeeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'StartMeeting', N'Start Meeting', N'بدء الاجتماع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'StartProcess')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'StartProcess', N'Start Process', N'بدء العملية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'StartTime')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'StartTime', N'Start Time', N'وقت البدء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Status')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Status', N'Status', N'الحالة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Step')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Step', N'Step', N'خطوة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Store')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Store', N'Store', N'المخزن');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Structure')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Structure', N'Structure', N'الهيكل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Structures')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Structures', N'Structures', N'الهياكل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SubCommittees')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SubCommittees', N'Sub Committees', N'اللجان');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SubCommitteesCount')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SubCommitteesCount', N'Sub Committees Count', N'عدد اللجان الفرعية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Subject')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Subject', N'Subject', N'الموضوع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SupportedFormats')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SupportedFormats', N'PDF, DOC, DOCX, XLS, XLSX', N'PDF, DOC, DOCX, XLS, XLSX');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'SystemDefault')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'SystemDefault', N'System Default', N'النظام');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TableName')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TableName', N'Table Name', N'اسم الجدول');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TableView')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TableView', N'Table View', N'عرض الجدول');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Tables')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Tables', N'Tables', N'الجداول');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TablesLinkedToTasks')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TablesLinkedToTasks', N'Tables Linked To Tasks', N'الجداول المرتبطة بالمهام');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Tags')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Tags', N'Tags', N'الوسوم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TaskOwner')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TaskOwner', N'Task Owner', N'مالك المهمة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Tasks')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Tasks', N'Tasks', N'المهام');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TasksConfiguration')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TasksConfiguration', N'Tasks Configuration', N'إعدادات المهام');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TeamsIntegrationDisabled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TeamsIntegrationDisabled', N'Teams Integration Disabled', N'تكامل Teams معطل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TeamsIntegrationDisabledDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TeamsIntegrationDisabledDesc', N'Teams Integration Disabled Desc', N'لن يتم إنشاء اجتماعات Teams تلقائياً. قم بتفعيل التكامل للبدء');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TeamsIntegrationEnabled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TeamsIntegrationEnabled', N'Teams Integration Enabled', N'تكامل Teams مفعل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TeamsIntegrationEnabledDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TeamsIntegrationEnabledDesc', N'Teams Integration Enabled Desc', N'سيتم إنشاء اجتماعات Teams تلقائياً للاجتماعات المحددة كاجتماعات عبر الإنترنت');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TemplateName')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TemplateName', N'Template Name', N'اسم القالب');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TemplateType')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TemplateType', N'Template Type', N'النوع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TenantId')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TenantId', N'Tenant Id', N'معرف المستأجر (Tenant ID)');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TenantIdDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TenantIdDesc', N'Tenant Id Desc', N'معرف المستأجر من Azure Portal');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TestFailed')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TestFailed', N'Test Failed', N'فشل الإرسال');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TestIntegration')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TestIntegration', N'Test Integration', N'اختبار التكامل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TestIntegrationDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TestIntegrationDesc', N'Test Integration Desc', N'اختبر تكامل Outlook عن طريق إرسال دعوة تقويم تجريبية لاجتماع موجود.');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TestSuccess')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TestSuccess', N'Test Success', N'تم الإرسال بنجاح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TestTeamsConnection')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TestTeamsConnection', N'Test Teams Connection', N'اختبار الاتصال');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TestTeamsConnectionDesc')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TestTeamsConnectionDesc', N'Test Teams Connection Desc', N'اختبار الاتصال بـ Microsoft Graph API للتأكد من صحة الإعدادات.');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TextField')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TextField', N'Text Field', N'حقل النص');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ThisMonth')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ThisMonth', N'This Month', N'هذا الشهر');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Time')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Time', N'Time', N'الوقت');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TimeRemaining')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TimeRemaining', N'Time Remaining', N'الوقت المتبقي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Timeline')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Timeline', N'Timeline', N'Timeline');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Title')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Title', N'Title', N'العنوان');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TitleTemplate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TitleTemplate', N'Title Template', N'قالب العنوان');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ToDate')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ToDate', N'To Date', N'إلى تاريخ');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Today')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Today', N'Today', N'Today');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TodaysMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TodaysMeetings', N'Todays Meetings', N'اجتماعات اليوم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Topics')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Topics', N'Topics', N'المحاور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Total')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Total', N'Total', N'الإجمالي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TotalAttendees')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TotalAttendees', N'Total Attendees', N'إجمالي الحضور');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TotalDepartments')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TotalDepartments', N'Total Departments', N'إجمالي الأقسام');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TotalDrafts')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TotalDrafts', N'Total Drafts', N'إجمالي المسودات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TotalEntries')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TotalEntries', N'Total Entries', N'إجمالي المصطلحات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TotalMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TotalMeetings', N'Total Meetings', N'إجمالي الاجتماعات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TotalPermissions')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TotalPermissions', N'Total Permissions', N'إجمالي الصلاحيات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TotalRecommendations')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TotalRecommendations', N'Total Recommendations', N'إجمالي التوصيات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TotalRecords')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TotalRecords', N'Total Records', N'إجمالي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TotalRoles')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TotalRoles', N'Total Roles', N'إجمالي الأدوار');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TotalSettings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TotalSettings', N'Total Settings', N'إجمالي الإعدادات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TotalTemplates')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TotalTemplates', N'Total Templates', N'إجمالي القوالب');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TotalTypes')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TotalTypes', N'Total Types', N'إجمالي الأنواع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TotalUsers')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TotalUsers', N'Total Users', N'إجمالي المستخدمين');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TryAdjustingFilters')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TryAdjustingFilters', N'Try Adjusting Filters', N'جرب تعديل معايير البحث');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TryDifferentSearch')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TryDifferentSearch', N'Try Different Search', N'جرب البحث بكلمات مختلفة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TwoFactorAuth')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TwoFactorAuth', N'Two Factor Auth', N'التحقق بخطوتين');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TwoFactorAuthMessage')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TwoFactorAuthMessage', N'Two Factor Auth Message', N'أدخل رمز التحقق المرسل إلى هاتفك');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TwoFactorHint')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TwoFactorHint', N'Two Factor Hint', N'أضف طبقة حماية إضافية لحسابك');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Type')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Type', N'Type', N'النوع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TypeMessage')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TypeMessage', N'Type Message', N'اكتب رسالة...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'TypeToSearch')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'TypeToSearch', N'Type To Search', N'اكتب للبحث...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'UnmappedMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'UnmappedMeetings', N'Unmapped Meetings', N'اجتماعات غير مرتبطة باللجان');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'UnsupportedFormat')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'UnsupportedFormat', N'Unsupported Format', N'تنسيق غير مدعوم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Upcoming')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Upcoming', N'Upcoming', N'المقبلة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'UpcomingMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'UpcomingMeetings', N'Upcoming Meetings', N'الاجتماعات القادمة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Update')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Update', N'Update', N'تحديث');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'UpdateSuccess')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'UpdateSuccess', N'Update Success', N'تم التحديث بنجاح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Updates')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Updates', N'Updates', N'تحديث');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Upload')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Upload', N'Upload', N'رفع ملف');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'UploadAttachment')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'UploadAttachment', N'Upload Attachment', N'رفع مرفق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'UploadNewPicture')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'UploadNewPicture', N'Upload New Picture', N'رفع صورة جديدة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Uploading')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Uploading', N'Uploading', N'جاري الرفع...');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'UppercaseLetter')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'UppercaseLetter', N'Uppercase Letter', N'حرف كبير واحد (A-Z)');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'User')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'User', N'User', N'المستخدم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'UserPermissions')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'UserPermissions', N'User Permissions', N'صلاحيات المستخدم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'UserReadAll')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'UserReadAll', N'User Read All', N'للوصول لمعلومات المستخدمين');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Username')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Username', N'Username', N'المستخدم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Users')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Users', N'Users', N'المستخدمين');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'UsersInDepartment')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'UsersInDepartment', N'Users In Department', N'مستخدمين في القسم');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'UsersInStructure')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'UsersInStructure', N'Users In Structure', N'المستخدمين في الهيكل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ValueField')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ValueField', N'Value Field', N'حقل القيمة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Verify')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Verify', N'Verify', N'تحقق');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'View')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'View', N'View', N'عرض');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ViewAll')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ViewAll', N'View All', N'عرض الكل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ViewAllMine')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ViewAllMine', N'View All Mine', N'عرض جميع توصياتي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ViewAndManageMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ViewAndManageMeetings', N'View And Manage Meetings', N'عرض وإدارة جميع الاجتماعات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ViewFile')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ViewFile', N'View File', N'عرض الملف');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ViewMeeting')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ViewMeeting', N'View Meeting', N'عرض');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'ViewMeetings')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'ViewMeetings', N'View Meetings', N'عرض الاجتماعات');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Viewed')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Viewed', N'Viewed', N'تمت المعاينة');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'VotingOptions')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'VotingOptions', N'Voting Options', N'خيارات التصويت');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'VotingType')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'VotingType', N'Voting Type', N'نوع التصويت');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Weekly')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Weekly', N'Weekly', N'Weekly');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'Welcome')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'Welcome', N'Welcome', N'مرحباً');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'WelcomeBack')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'WelcomeBack', N'Welcome Back', N'Welcome Back');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'WhatToExpect')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'WhatToExpect', N'What To Expect', N'ماذا تتوقع');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'WorkflowDefinition')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'WorkflowDefinition', N'Workflow Definition', N'تعريف سير العمل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'WorkflowDesigner')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'WorkflowDesigner', N'Workflow Designer', N'مصمم سير العمل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'WorkflowDesignerCanvas')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'WorkflowDesignerCanvas', N'Workflow Designer Canvas', N'لوحة تصميم سير العمل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'WorkflowProperties')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'WorkflowProperties', N'Workflow Properties', N'خصائص سير العمل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'WorkflowPropertiesConfig')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'WorkflowPropertiesConfig', N'Workflow Properties Config', N'إعدادات خصائص سير العمل');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'WorkflowPublished')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'WorkflowPublished', N'Workflow Published', N'تم نشر سير العمل بنجاح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'WorkflowStarted')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'WorkflowStarted', N'Workflow Started', N'تم بدء العملية بنجاح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'act')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'act', N'', N'');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'att')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'att', N'0', N'0');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'emailDisabled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'emailDisabled', N'email Disabled', N'تم تعطيل البريد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'emailEnabled')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'emailEnabled', N'email Enabled', N'تم تفعيل البريد');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'hvd')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'hvd', N'', N'');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'process-number')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'process-number', N'process number', N'رقم العملية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'reference-number')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'reference-number', N'reference number', N'الرقم المرجعي');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'resources_homepage')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'resources_homepage', N'resources homepage', N'الصفحة الرئيسية');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'resources_success_start_process')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'resources_success_start_process', N'resources success start process', N'تم بدء العملية بنجاح');
IF NOT EXISTS (SELECT 1 FROM [Dictionary] WHERE [Keyword] = N'task')
INSERT INTO [Dictionary] ([Keyword], [EN], [AR]) VALUES (N'task', N'0', N'0');
