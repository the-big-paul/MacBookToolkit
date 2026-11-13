using System;
using System.Collections.Generic;

public static class Texts
{
    public static string Lang = "ru";

    static Dictionary<string, string> ru = new Dictionary<string, string>()
    {
        {"sec_display","Дисплей и яркость"},
        {"sec_display_left","Дисплей"},
        {"sec_display_and"," и "},
        {"sec_display_right","яркость"},
        {"sec_key","Клавиатура"},
        {"sec_touch","Трекпад"},
        {"sec_options","Опции"},

        {"chk_screen","Затемнять экран при простое"},
        {"lbl_idle","Простой до затемнения"},
        {"lbl_dim","Яркость затемнения"},
        {"lbl_normal","Яркость при активности"},
        {"lbl_screenoff","Отключать дисплей через"},

        {"chk_touch","Отключение трекпада при печати"},
        {"lbl_block","Пауза трекпада после клавиши"},

        {"chk_key","Убирать двойные буквы"},
        {"lbl_up","Пауза при отпускании"},
        {"lbl_down","Защита быстрой печати"},

        {"lbl_theme","Тема оформления"},
        {"theme_system","Как в Windows"},
        {"theme_light","Светлая"},
        {"theme_dark","Тёмная"},

        {"lbl_accent","Цвет акцента"},
        {"accent_system","Как в Windows"},
        {"accent_blue","Синий"},
        {"accent_purple","Фиолетовый"},
        {"accent_pink","Розовый"},
        {"accent_red","Красный"},
        {"accent_orange","Оранжевый"},
        {"accent_yellow","Жёлтый"},
        {"accent_green","Зелёный"},
        {"accent_teal","Бирюзовый"},
        {"accent_gray","Серый"},

        {"chk_topmost","Поверх всех окон"},
        {"chk_autostart","Запускать вместе с Windows"},
        {"chk_tray","Показывать иконку в трее"},
        {"lbl_lang","Язык интерфейса"},

        {"unit_sec","с"},
        {"unit_ms","мс"},
        {"unit_min","мин"},
        {"unit_pct","%"},

        {"btn_save","Сохранить"},
        {"btn_reset","Сбросить настройки"},
        {"reset_title","Сбросить настройки"},
        {"reset_confirm","Вернуть все настройки к рекомендованным?\n\n• Затемнение: простой 15 с → 10 %\n• Яркость при активности: 100 %\n• Трекпад: пауза 700 мс\n• Клавиатура: пауза 70 / 20 мс\n• Отключать дисплей через: 2 мин\n\nЯзык, тема и цвет акцента сохранятся."},
        {"reset_done","Готово: настройки сброшены к рекомендованным."},

        {"tray_show","Открыть окно"},
        {"tray_topmost","Поверх всех окон"},
        {"tray_autostart","Запускать вместе с Windows"},
        {"tray_exit","Выход"},
        {"tray_min","Свёрнуто в трей. Программа продолжает работать."},
        {"already_running","MacBookToolkit уже запущен — окно восстановлено."},
        {"hung_title","Программа не отвечает"},
        {"hung_body","Запущенный экземпляр MacBookToolkit не отвечает на запросы.\n\nЗавершить его и запустить программу заново?"},
        {"version_tip","Обновления и описание программы — в репозитории GitHub.\n© The Big Paul"},

        {"tip_chk_screen","Когда вы не трогаете мышь и клавиатуру дольше выбранного времени, яркость экрана снижается. При любом действии она сразу возвращается к обычной.\nРекомендация: 15–20 с."},
        {"tip_idle","Через сколько секунд бездействия начинать затемнение.\nРекомендация: 15 с — быстро, 30–60 с — если часто читаете с экрана."},
        {"tip_dim","До какой яркости затемнять экран. 10 % — почти невидимо в тёмной комнате, 30–50 % — комфортно днём.\nРекомендация: 10 % вечером, 30 % днём."},
        {"tip_normal","Яркость, которая включается, когда вы работаете (активность).\nРекомендация: 100 %."},
        {"tip_screenoff","Через сколько минут бездействия Windows отключит дисплей.\nРекомендация: 2 минуты — баланс батареи и удобства; 5 минут — если смотрите видео."},
        {"tip_chk_touch","Пока вы печатаете, трекпад не реагирует на касания: ладонь не сдвинет курсор и не нажмёт кнопку случайно."},
        {"tip_block","Сколько миллисекунд после нажатия клавиши трекпад не реагирует на касания.\nРекомендация: 500–700 мс."},
        {"tip_chk_key","Убирает двойные буквы на клавиатуре: если одна и та же клавиша срабатывает слишком быстро, второе нажатие отбрасывается."},
        {"tip_up","Если ту же клавишу нажать быстрее этого времени после отпускания, нажатие считается дребезгом и удаляется.\nРекомендация: 70 мс; если часто теряются быстрые повторы — 40–50 мс."},
        {"tip_down","Защита при быстрой печати: второе нажатие той же клавиши, пришедшее слишком быстро без отпускания, считается дребезгом и отбрасывается.\nРекомендация: 15–20 мс, не больше 30 мс — иначе при удержании клавиши перестанет работать авто-повтор."},
        {"tip_topmost","Окно программы останется поверх остальных приложений, пока вы не отключите этот режим."},
        {"tip_theme","«Как в Windows» — автоматически повторять системную светлую или тёмную тему. «Светлая» или «Тёмная» — принудительно, без слежения за Windows."},
        {"tip_accent","Цвет акцента используют кнопки, галочки и полосы выбора. «Как в Windows» берёт цвет из системных настроек."},
        {"tip_autostart","Запускать программу автоматически при входе в Windows."},
        {"tip_tray","Иконка программы в системном трее. Левый клик открывает окно, правый — меню с автозагрузкой и выходом."},
        {"tip_lang","Язык интерфейса и подсказок: русский, украинский или английский."},
        {"tip_save","Сохранить настройки и сразу применить их к системе."},
        {"tip_reset","Вернуть все настройки к рекомендованным значениям. Язык, тема и акцент останутся прежними."}
    };

    static Dictionary<string, string> uk = new Dictionary<string, string>()
    {
        {"sec_display","Дисплей і яскравість"},
        {"sec_display_left","Дисплей"},
        {"sec_display_and"," і "},
        {"sec_display_right","яскравість"},
        {"sec_key","Клавіатура"},
        {"sec_touch","Трекпад"},
        {"sec_options","Опції"},

        {"chk_screen","Затемнювати екран при простої"},
        {"lbl_idle","Простій до затемнення"},
        {"lbl_dim","Яскравість затемнення"},
        {"lbl_normal","Яскравість при активності"},
        {"lbl_screenoff","Вимикати дисплей через"},

        {"chk_touch","Вимкнення трекпада під час друку"},
        {"lbl_block","Пауза трекпада після клавіші"},

        {"chk_key","Прибирати подвійні літери"},
        {"lbl_up","Пауза при відпусканні"},
        {"lbl_down","Захист швидкого друку"},

        {"lbl_theme","Тема оформлення"},
        {"theme_system","Як у Windows"},
        {"theme_light","Світла"},
        {"theme_dark","Темна"},

        {"lbl_accent","Колір акценту"},
        {"accent_system","Як у Windows"},
        {"accent_blue","Синій"},
        {"accent_purple","Фіолетовий"},
        {"accent_pink","Рожевий"},
        {"accent_red","Червоний"},
        {"accent_orange","Помаранчевий"},
        {"accent_yellow","Жовтий"},
        {"accent_green","Зелений"},
        {"accent_teal","Бірюзовий"},
        {"accent_gray","Сірий"},

        {"chk_topmost","Поверх усіх вікон"},
        {"chk_autostart","Запускати разом із Windows"},
        {"chk_tray","Показувати іконку в треї"},
        {"lbl_lang","Мова інтерфейсу"},

        {"unit_sec","с"},
        {"unit_ms","мс"},
        {"unit_min","хв"},
        {"unit_pct","%"},

        {"btn_save","Зберегти"},
        {"btn_reset","Скинути налаштування"},
        {"reset_title","Скинути налаштування"},
        {"reset_confirm","Повернути всі налаштування до рекомендованих?\n\n• Затемнення: простій 15 с → 10 %\n• Яскравість при активності: 100 %\n• Трекпад: пауза 700 мс\n• Клавіатура: пауза 70 / 20 мс\n• Вимикати дисплей через: 2 хв\n\nМова, тема та колір акценту збережуться."},
        {"reset_done","Готово: налаштування скинуто до рекомендованих."},

        {"tray_show","Відкрити вікно"},
        {"tray_topmost","Поверх усіх вікон"},
        {"tray_autostart","Запускати разом із Windows"},
        {"tray_exit","Вихід"},
        {"tray_min","Згорнуто в трей. Програма продовжує працювати."},
        {"already_running","MacBookToolkit вже запущено — вікно відновлено."},
        {"hung_title","Програма не відповідає"},
        {"hung_body","Запущений екземпляр MacBookToolkit не відповідає на запити.\n\nЗавершити його та запустити програму заново?"},
        {"version_tip","Оновлення та опис програми — у репозиторії GitHub.\n© The Big Paul"},

        {"tip_chk_screen","Коли ви не торкаєтесь миші й клавіатури довше вибраного часу, яскравість екрана знижується. За будь-якої дії вона одразу повертається до звичайної.\nРекомендація: 15–20 с."},
        {"tip_idle","Через скільки секунд бездіяльності починати затемнення.\nРекомендація: 15 с — швидко, 30–60 с — якщо часто читаєте з екрана."},
        {"tip_dim","До якої яскравості затемнювати екран. 10 % — майже невидимо в темній кімнаті, 30–50 % — комфортно вдень.\nРекомендація: 10 % увечері, 30 % удень."},
        {"tip_normal","Яскравість, яка вмикається, коли ви працюєте (активність).\nРекомендація: 100 %."},
        {"tip_screenoff","Через скільки хвилин бездіяльності Windows вимкне дисплей.\nРекомендація: 2 хвилини — баланс батареї та зручності; 5 хвилин — якщо дивитесь відео."},
        {"tip_chk_touch","Поки ви друкуєте, трекпад не реагує на торкання: долоня не зсуне курсор і не натисне кнопку випадково."},
        {"tip_block","Скільки мілісекунд після натискання клавіші трекпад не реагує на торкання.\nРекомендація: 500–700 мс."},
        {"tip_chk_key","Прибирає подвійні літери: якщо та сама клавіша спрацьовує занадто швидко, друге натискання відкидається."},
        {"tip_up","Якщо ту саму клавішу натиснути швидше цього часу після відпускання, натискання вважається брязкотом і видаляється.\nРекомендація: 70 мс; якщо часто губляться швидкі повтори — 40–50 мс."},
        {"tip_down","Захист під час швидкого друку: друге натискання тієї самої клавіші, що прийшло занадто швидко без відпускання, вважається брязкотом і відкидається.\nРекомендація: 15–20 мс, не більше 30 мс — інакше під час утримання клавіші перестане працювати автоповтор."},
        {"tip_topmost","Вікно програми залишиться поверх інших застосунків, доки ви не вимкнете цей режим."},
        {"tip_theme","«Як у Windows» — автоматично повторювати системну світлу чи темну тему. «Світла» або «Темна» — примусово, без стеження за Windows."},
        {"tip_accent","Колір акценту використовують кнопки, позначки та смуги вибору. «Як у Windows» бере колір із системних налаштувань."},
        {"tip_autostart","Запускати програму автоматично при вході в Windows."},
        {"tip_tray","Іконка програми в системному треї. Лівий клік відкриває вікно, правий — меню з автозапуском і виходом."},
        {"tip_lang","Мова інтерфейсу та підказок: українська, російська або англійська."},
        {"tip_save","Зберегти налаштування й одразу застосувати їх до системи."},
        {"tip_reset","Повернути всі налаштування до рекомендованих значень. Мова, тема та акцент залишаться попередніми."}
    };

    static Dictionary<string, string> en = new Dictionary<string, string>()
    {
        {"sec_display","Display & brightness"},
        {"sec_display_left","Display"},
        {"sec_display_and"," & "},
        {"sec_display_right","brightness"},
        {"sec_key","Keyboard"},
        {"sec_touch","Trackpad"},
        {"sec_options","Options"},

        {"chk_screen","Dim the screen when idle"},
        {"lbl_idle","Idle time before dimming"},
        {"lbl_dim","Dimmed brightness"},
        {"lbl_normal","Active brightness"},
        {"lbl_screenoff","Turn the display off after"},

        {"chk_touch","Disable trackpad while typing"},
        {"lbl_block","Trackpad pause after keypress"},

        {"chk_key","Remove double-typed letters"},
        {"lbl_up","Pause on key release"},
        {"lbl_down","Fast-typing protection"},

        {"lbl_theme","Theme"},
        {"theme_system","Follow Windows"},
        {"theme_light","Light"},
        {"theme_dark","Dark"},

        {"lbl_accent","Accent colour"},
        {"accent_system","Follow Windows"},
        {"accent_blue","Blue"},
        {"accent_purple","Purple"},
        {"accent_pink","Pink"},
        {"accent_red","Red"},
        {"accent_orange","Orange"},
        {"accent_yellow","Yellow"},
        {"accent_green","Green"},
        {"accent_teal","Teal"},
        {"accent_gray","Gray"},

        {"chk_topmost","Always on top"},
        {"chk_autostart","Start with Windows"},
        {"chk_tray","Show tray icon"},
        {"lbl_lang","Interface language"},

        {"unit_sec","s"},
        {"unit_ms","ms"},
        {"unit_min","min"},
        {"unit_pct","%"},

        {"btn_save","Save"},
        {"btn_reset","Reset settings"},
        {"reset_title","Reset settings"},
        {"reset_confirm","Restore all settings to the recommended values?\n\n• Dimming: 15 s idle → 10 %\n• Active brightness: 100 %\n• Trackpad: 700 ms pause\n• Keyboard: 70 / 20 ms pause\n• Turn the display off after: 2 min\n\nLanguage, theme and accent are kept."},
        {"reset_done","Done — settings restored to the recommended values."},

        {"tray_show","Open window"},
        {"tray_topmost","Always on top"},
        {"tray_autostart","Start with Windows"},
        {"tray_exit","Exit"},
        {"tray_min","Minimized to tray. The program keeps running."},
        {"already_running","MacBookToolkit is already running — window restored."},
        {"hung_title","The application is not responding"},
        {"hung_body","The running MacBookToolkit instance is not responding.\n\nTerminate it and start the program again?"},
        {"version_tip","Updates and program description — in the GitHub repository.\n© The Big Paul"},

        {"tip_chk_screen","When you leave the mouse and keyboard alone longer than the set time, the screen dims. Any input instantly restores normal brightness.\nRecommendation: 15–20 s."},
        {"tip_idle","Seconds of inactivity before dimming starts.\nRecommendation: 15 s for quick dimming, 30–60 s if you often read on screen."},
        {"tip_dim","Brightness level to dim to. 10 % is almost invisible in a dark room; 30–50 % is comfortable in daylight.\nRecommendation: 10 % in the evening, 30 % in the day."},
        {"tip_normal","Brightness used while you are active.\nRecommendation: 100 %."},
        {"tip_screenoff","Minutes of inactivity before Windows turns the display off.\nRecommendation: 2 minutes balances battery and convenience; 5 minutes if you watch video."},
        {"tip_chk_touch","While you type, the trackpad ignores contact: your palm will not move the cursor or click accidentally."},
        {"tip_block","Milliseconds after a key press during which the trackpad ignores touches.\nRecommendation: 500–700 ms."},
        {"tip_chk_key","Removes double letters: if the same key fires again too quickly, the second press is dropped."},
        {"tip_up","If the same key is pressed faster than this after release, the press is treated as chatter and removed.\nRecommendation: 70 ms; drop to 40–50 ms if fast repeats are often lost."},
        {"tip_down","Fast-typing protection: a second press of the same key arriving too quickly without release counts as chatter and is dropped.\nRecommendation: 15–20 ms, never above 30 ms, otherwise holding a key stops auto-repeating."},
        {"tip_topmost","The program window stays above other applications until you turn this off."},
        {"tip_theme","Follow Windows — automatically match the system light/dark theme. Light or Dark — force it, without following Windows."},
        {"tip_accent","The accent colour is used by buttons, check marks and selection bars. Follow Windows takes the colour from system settings."},
        {"tip_autostart","Start the program automatically at Windows login."},
        {"tip_tray","Program icon in the system tray. Left click opens the window, right click opens the menu with autostart and exit."},
        {"tip_lang","Interface and tooltip language: English, Russian or Ukrainian."},
        {"tip_save","Save the settings and apply them to the system immediately."},
        {"tip_reset","Restore all settings to the recommended values. Language, theme and accent are kept."}
    };

    public static string T(string key)
    {
        Dictionary<string, string> d = ru;
        if (Lang == "en") d = en;
        else if (Lang == "uk") d = uk;
        string v;
        if (d.TryGetValue(key, out v)) return v;
        return key;
    }
}
