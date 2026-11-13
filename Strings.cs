using System;
using System.Collections.Generic;

public static class Texts
{
    public static string Lang = "ru";

    static Dictionary<string, string> ru = new Dictionary<string, string>()
    {
        {"sec_screen","🖥️ Экран и яркость"},
        {"sec_key","⌨️ Клавиатура"},
        {"sec_touch","🖱️ Тачпад"},
        {"sec_power","🔋 Питание"},
        {"sec_window","⚙️ Окно и система"},

        {"chk_screen","🌙 Затемнять экран при простое"},
        {"lbl_idle","⏳ Простой до затемнения, сек"},
        {"lbl_dim","🔅 Яркость затемнения, %"},
        {"lbl_normal","🔆 Яркость при работе, %"},

        {"chk_touch","✋ Тачпад не мешает при печати"},
        {"lbl_block","⏲️ Блокировка после клавиши, мс"},

        {"chk_key","🛠️ Убирать двойные буквы"},
        {"lbl_up","⏱️ Пауза после отпускания, мс"},
        {"lbl_down","⏱️ Пауза между нажатиями, мс"},

        {"lbl_screenoff","🔌 Экран гаснет через, мин"},

        {"chk_topmost","📌 Окно поверх всех окон"},
        {"lbl_theme","🎨 Тема оформления"},
        {"theme_system","🔄 Как в Windows"},
        {"theme_light","☀️ Светлая"},
        {"theme_dark","🌑 Тёмная"},
        {"chk_autostart","🚀 Запускать вместе с Windows"},
        {"chk_tray","📥 Показывать иконку в трее"},
        {"lbl_lang","🌍 Язык интерфейса"},

        {"btn_save","💾 Сохранить"},
        {"btn_reset","↩️ Сбросить настройки"},
        {"reset_title","Сбросить настройки"},
        {"reset_confirm","Вернуть все настройки к рекомендованным?\n\n🌗 Затемнение: простой 15 с → 10 %\n🌈 Яркость при работе: 100 %\n✋ Тачпад: блокировка 700 мс\n⌨️ Клавиатура: пауза 70 / 25 мс\n🔌 Экран: отключать через 2 мин\n\nЯзык и тема оформления сохранятся."},
        {"reset_done","Готово: настройки сброшены к рекомендованным."},

        {"tray_show","🪟 Открыть окно"},
        {"tray_topmost","📌 Поверх всех окон"},
        {"tray_autostart","🚀 Запускать вместе с Windows"},
        {"tray_exit","🚪 Выход"},
        {"tray_min","Свёрнуто в трей. Программа продолжает работать."},
        {"already_running","MacBookToolkit уже запущен — окно восстановлено."},
        {"hung_title","Программа не отвечает"},
        {"hung_body","Запущенный экземпляр MacBookToolkit не отвечает на запросы.\n\nЗавершить его и запустить программу заново?"},

        {"tip_chk_screen","Когда вы не трогаете мышь и клавиатуру дольше выбранного времени, яркость экрана снижается. При любом действии она сразу возвращается к обычной."},
        {"tip_idle","Через сколько секунд бездействия начинать затемнение. Например, 15 — через 15 секунд после последнего движения мыши или нажатия клавиши."},
        {"tip_dim","До какой яркости затемнять экран. 10 % — почти невидимо в тёмной комнате, 30–50 % — комфортно днём."},
        {"tip_normal","Яркость, которая включается, когда вы работаете. Обычно 100 % — максимальная."},
        {"tip_chk_touch","Пока вы печатаете, касания тачпада игнорируются: ладонь не сдвинет курсор и не нажмёт кнопку случайно."},
        {"tip_block","Сколько миллисекунд после нажатия клавиши тачпад не реагирует на касания. 700 мс — примерно короткая пауза между словами."},
        {"tip_chk_key","Убирает двойные буквы: если одна и та же клавиша срабатывает слишком быстро, второе нажатие отбрасывается."},
        {"tip_up","Если ту же клавишу нажать быстрее этого времени после отпускания, нажатие считается дребезгом и удаляется. 70 мс — безопасный баланс."},
        {"tip_down","Пауза между двумя нажатиями одной клавиши без отпускания. Не ставьте больше 30 мс — иначе перестанет работать авто-повтор при удержании."},
        {"tip_screenoff","Через сколько минут бездействия Windows выключит экран. 2 минуты — разумный баланс между экономией батареи и удобством."},
        {"tip_topmost","Окно программы останется поверх остальных приложений, пока вы не отключите этот режим."},
        {"tip_theme","«Как в Windows» — автоматически повторять системную светлую или тёмную тему и акцентный цвет. «Светлая» или «Тёмная» — принудительно."},
        {"tip_autostart","Запускать программу автоматически при входе в Windows."},
        {"tip_tray","Иконка программы в системном трее. Левый клик открывает окно, правый — меню с автозагрузкой и выходом."},
        {"tip_lang","Язык интерфейса: русский, украинский или английский."},
        {"tip_save","Сохранить настройки и сразу применить их к системе."},
        {"tip_reset","Вернуть все настройки к рекомендованным значениям. Язык и тема оформления останутся прежними."}
    };

    static Dictionary<string, string> uk = new Dictionary<string, string>()
    {
        {"sec_screen","🖥️ Екран і яскравість"},
        {"sec_key","⌨️ Клавіатура"},
        {"sec_touch","🖱️ Тачпад"},
        {"sec_power","🔋 Живлення"},
        {"sec_window","⚙️ Вікно і система"},

        {"chk_screen","🌙 Затемнювати екран при простої"},
        {"lbl_idle","⏳ Простій до затемнення, с"},
        {"lbl_dim","🔅 Яскравість затемнення, %"},
        {"lbl_normal","🔆 Яскравість під час роботи, %"},

        {"chk_touch","✋ Тачпад не заважає під час друку"},
        {"lbl_block","⏲️ Блокування після клавіші, мс"},

        {"chk_key","🛠️ Прибирати подвійні літери"},
        {"lbl_up","⏱️ Пауза після відпускання, мс"},
        {"lbl_down","⏱️ Пауза між натисканнями, мс"},

        {"lbl_screenoff","🔌 Екран гасне через, хв"},

        {"chk_topmost","📌 Вікно поверх усіх вікон"},
        {"lbl_theme","🎨 Тема оформлення"},
        {"theme_system","🔄 Як у Windows"},
        {"theme_light","☀️ Світла"},
        {"theme_dark","🌑 Темна"},
        {"chk_autostart","🚀 Запускати разом із Windows"},
        {"chk_tray","📥 Показувати іконку в треї"},
        {"lbl_lang","🌍 Мова інтерфейсу"},

        {"btn_save","💾 Зберегти"},
        {"btn_reset","↩️ Скинути налаштування"},
        {"reset_title","Скинути налаштування"},
        {"reset_confirm","Повернути всі налаштування до рекомендованих?\n\n🌗 Затемнення: простій 15 с → 10 %\n🌈 Яскравість під час роботи: 100 %\n✋ Тачпад: блокування 700 мс\n⌨️ Клавіатура: пауза 70 / 25 мс\n🔌 Екран: вимикати через 2 хв\n\nМова та тема оформлення збережуться."},
        {"reset_done","Готово: налаштування скинуто до рекомендованих."},

        {"tray_show","🪟 Відкрити вікно"},
        {"tray_topmost","📌 Поверх усіх вікон"},
        {"tray_autostart","🚀 Запускати разом із Windows"},
        {"tray_exit","🚪 Вихід"},
        {"tray_min","Згорнуто в трей. Програма продовжує працювати."},
        {"already_running","MacBookToolkit вже запущено — вікно відновлено."},
        {"hung_title","Програма не відповідає"},
        {"hung_body","Запущений екземпляр MacBookToolkit не відповідає на запити.\n\nЗавершити його та запустити програму заново?"},

        {"tip_chk_screen","Коли ви не торкаєтесь миші й клавіатури довше вибраного часу, яскравість екрана знижується. За будь-якої дії вона одразу повертається до звичайної."},
        {"tip_idle","Через скільки секунд бездіяльності починати затемнення. Наприклад, 15 — через 15 секунд після останнього руху миші чи натискання клавіші."},
        {"tip_dim","До якої яскравості затемнювати екран. 10 % — майже невидимо в темній кімнаті, 30–50 % — комфортно вдень."},
        {"tip_normal","Яскравість, яка вмикається, коли ви працюєте. Зазвичай 100 % — максимальна."},
        {"tip_chk_touch","Поки ви друкуєте, торкання тачпада ігноруються: долоня не зсуне курсор і не натисне кнопку випадково."},
        {"tip_block","Скільки мілісекунд після натискання клавіші тачпад не реагує на торкання. 700 мс — приблизно коротка пауза між словами."},
        {"tip_chk_key","Прибирає подвійні літери: якщо та сама клавіша спрацьовує занадто швидко, друге натискання відкидається."},
        {"tip_up","Якщо ту саму клавішу натиснути швидше цього часу після відпускання, натискання вважається брязкотом і видаляється. 70 мс — безпечний баланс."},
        {"tip_down","Пауза між двома натисканнями однієї клавіші без відпускання. Не ставте більше 30 мс — інакше перестане працювати автоповтор під час утримання."},
        {"tip_screenoff","Через скільки хвилин бездіяльності Windows вимкне екран. 2 хвилини — розумний баланс між економією батареї та зручністю."},
        {"tip_topmost","Вікно програми залишиться поверх інших застосунків, доки ви не вимкнете цей режим."},
        {"tip_theme","«Як у Windows» — автоматично повторювати системну світлу чи темну тему й акцентний колір. «Світла» або «Темна» — примусово."},
        {"tip_autostart","Запускати програму автоматично при вході в Windows."},
        {"tip_tray","Іконка програми в системному треї. Лівий клік відкриває вікно, правий — меню з автозапуском і виходом."},
        {"tip_lang","Мова інтерфейсу: українська, російська або англійська."},
        {"tip_save","Зберегти налаштування й одразу застосувати їх до системи."},
        {"tip_reset","Повернути всі налаштування до рекомендованих значень. Мова та тема оформлення залишаться попередніми."}
    };

    static Dictionary<string, string> en = new Dictionary<string, string>()
    {
        {"sec_screen","🖥️ Screen & brightness"},
        {"sec_key","⌨️ Keyboard"},
        {"sec_touch","🖱️ Touchpad"},
        {"sec_power","🔋 Power"},
        {"sec_window","⚙️ Window & system"},

        {"chk_screen","🌙 Dim the screen when idle"},
        {"lbl_idle","⏳ Idle time before dimming, s"},
        {"lbl_dim","🔅 Dimmed brightness, %"},
        {"lbl_normal","🔆 Working brightness, %"},

        {"chk_touch","✋ Ignore touchpad while typing"},
        {"lbl_block","⏲️ Touchpad block after key, ms"},

        {"chk_key","🛠️ Remove double-typed letters"},
        {"lbl_up","⏱️ Pause after key release, ms"},
        {"lbl_down","⏱️ Pause between presses, ms"},

        {"lbl_screenoff","🔌 Screen turns off after, min"},

        {"chk_topmost","📌 Always on top"},
        {"lbl_theme","🎨 Theme"},
        {"theme_system","🔄 Follow Windows"},
        {"theme_light","☀️ Light"},
        {"theme_dark","🌑 Dark"},
        {"chk_autostart","🚀 Start with Windows"},
        {"chk_tray","📥 Show tray icon"},
        {"lbl_lang","🌍 Interface language"},

        {"btn_save","💾 Save"},
        {"btn_reset","↩️ Reset settings"},
        {"reset_title","Reset settings"},
        {"reset_confirm","Restore all settings to the recommended values?\n\n🌗 Dimming: 15 s idle → 10 %\n🌈 Working brightness: 100 %\n✋ Touchpad: 700 ms block\n⌨️ Keyboard: 70 / 25 ms pause\n🔌 Screen: off after 2 min\n\nLanguage and theme are kept."},
        {"reset_done","Done — settings restored to the recommended values."},

        {"tray_show","🪟 Open window"},
        {"tray_topmost","📌 Always on top"},
        {"tray_autostart","🚀 Start with Windows"},
        {"tray_exit","🚪 Exit"},
        {"tray_min","Minimized to tray. The program keeps running."},
        {"already_running","MacBookToolkit is already running — window restored."},
        {"hung_title","The application is not responding"},
        {"hung_body","The running MacBookToolkit instance is not responding.\n\nTerminate it and start the program again?"},

        {"tip_chk_screen","When you leave the mouse and keyboard alone longer than the set time, the screen dims. Any input instantly restores normal brightness."},
        {"tip_idle","Seconds of inactivity before dimming starts. For example, 15 means 15 seconds after the last mouse move or key press."},
        {"tip_dim","Brightness level to dim to. 10 % is almost invisible in a dark room; 30–50 % is comfortable in daylight."},
        {"tip_normal","Brightness used while you work. Usually 100 % — the maximum."},
        {"tip_chk_touch","While you type, touchpad contact is ignored: your palm will not move the cursor or click accidentally."},
        {"tip_block","Milliseconds after a key press during which the touchpad ignores touches. 700 ms is about a short pause between words."},
        {"tip_chk_key","Removes double letters: if the same key fires again too quickly, the second press is dropped."},
        {"tip_up","If the same key is pressed faster than this after release, the press is treated as chatter and removed. 70 ms is a safe balance."},
        {"tip_down","Pause between two presses of one key without releasing it. Do not exceed 30 ms, otherwise holding a key stops auto-repeating."},
        {"tip_screenoff","Minutes of inactivity before Windows turns the screen off. 2 minutes is a good balance between battery saving and convenience."},
        {"tip_topmost","The program window stays above other applications until you turn this off."},
        {"tip_theme","Follow Windows — automatically match the system light/dark theme and accent colour. Light or Dark — force it."},
        {"tip_autostart","Start the program automatically at Windows login."},
        {"tip_tray","Program icon in the system tray. Left click opens the window, right click opens the menu with autostart and exit."},
        {"tip_lang","Interface language: English, Russian or Ukrainian."},
        {"tip_save","Save the settings and apply them to the system immediately."},
        {"tip_reset","Restore all settings to the recommended values. Language and theme are kept."}
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
