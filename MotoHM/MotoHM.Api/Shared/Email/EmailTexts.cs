namespace MotoHM.Api.Shared.Email;

internal sealed class EmailTexts
{
    public required string Lang { get; init; }

    public required string VerifySubject { get; init; }
    public required string VerifyHeading { get; init; }
    public required string VerifyIntro { get; init; }

    public required string ResetSubject { get; init; }
    public required string ResetHeading { get; init; }
    public required string ResetIntro { get; init; }

    public required string Greeting { get; init; }
    public required string Preview { get; init; }      // {0} = kod, {1} = deqiqe
    public required string ValidText { get; init; }    // {0} = deqiqe
    public required string SecurityTitle { get; init; }
    public required string SecurityText { get; init; }
    public required string Footer { get; init; }

    public static EmailTexts Get(string? lang) => lang switch
    {
        "en" => En,
        "ru" => Ru,
        _ => Az
    };

    private static readonly EmailTexts Az = new()
    {
        Lang = "az",
        VerifySubject = "MotoHM - Email təsdiq kodu",
        VerifyHeading = "Email təsdiqi",
        VerifyIntro = "Qeydiyyatı tamamlamaq üçün aşağıdakı təsdiq kodundan istifadə edin:",
        ResetSubject = "MotoHM - Şifrə bərpa kodu",
        ResetHeading = "Şifrə bərpası",
        ResetIntro = "Şifrənizi bərpa etmək üçün aşağıdakı təsdiq kodundan istifadə edin:",
        Greeting = "Salam",
        Preview = "Təsdiq kodunuz: {0}. Kod {1} dəqiqə etibarlıdır.",
        ValidText = "Bu kod {0} dəqiqə ərzində etibarlıdır.",
        SecurityTitle = "Təhlükəsizlik xatırlatması:",
        SecurityText = "Bu kodu heç kimlə paylaşmayın. MotoHM əməkdaşları sizdən heç vaxt kod istəməz. Bu tələbi siz göndərməmisinizsə, bu mesajı nəzərə almayın.",
        Footer = "Bu mesaj MotoHM tərəfindən avtomatik göndərilib. Zəhmət olmasa cavab yazmayın."
    };

    private static readonly EmailTexts En = new()
    {
        Lang = "en",
        VerifySubject = "MotoHM - Email verification code",
        VerifyHeading = "Verify your email",
        VerifyIntro = "Use the verification code below to complete your registration:",
        ResetSubject = "MotoHM - Password reset code",
        ResetHeading = "Reset your password",
        ResetIntro = "Use the verification code below to reset your password:",
        Greeting = "Hello",
        Preview = "Your verification code: {0}. Valid for {1} minutes.",
        ValidText = "This code is valid for {0} minutes.",
        SecurityTitle = "Security reminder:",
        SecurityText = "Never share this code with anyone. MotoHM staff will never ask you for it. If you did not make this request, please ignore this message.",
        Footer = "This message was sent automatically by MotoHM. Please do not reply."
    };

    private static readonly EmailTexts Ru = new()
    {
        Lang = "ru",
        VerifySubject = "MotoHM - Код подтверждения email",
        VerifyHeading = "Подтверждение email",
        VerifyIntro = "Для завершения регистрации используйте код подтверждения ниже:",
        ResetSubject = "MotoHM - Код для восстановления пароля",
        ResetHeading = "Восстановление пароля",
        ResetIntro = "Для восстановления пароля используйте код подтверждения ниже:",
        Greeting = "Здравствуйте",
        Preview = "Ваш код подтверждения: {0}. Действителен {1} мин.",
        ValidText = "Код действителен {0} мин.",
        SecurityTitle = "Напоминание о безопасности:",
        SecurityText = "Никому не сообщайте этот код. Сотрудники MotoHM никогда не запрашивают его. Если вы не отправляли этот запрос, просто проигнорируйте это письмо.",
        Footer = "Это письмо отправлено автоматически. Пожалуйста, не отвечайте на него."
    };
}