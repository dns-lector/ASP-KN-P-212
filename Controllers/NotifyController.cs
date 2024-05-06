using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_KN_P_212.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
    }
}
/* Робота з E-mail
 * 1. SMTP - Simple Mail Transfer Protocol
 * протокол надсилання E-mail
 *  [Email-server]---->[Email-server]
 *      SMTP/send      IMAP\receive
 * [Backend]               [User-mailbox]
 * Для роботи з E-mail необхідно створити поштову скриню (mailbox)
 * на поштовому сервісі, який підтримує SMTP (у бажаному тарифному плані)
 * Далі на прикладі Gmail
 * 
 * 2. Організація налаштувань
 * !!! Паролі від пошти (та інших ресурсів) не можна розміщувати на 
 * відкритих репозиторіях і не бажано навіть на закритих.
 * Задля цього створюються два файли конфігурації - один реальний, який
 *  вилучається з репозиторію, інший - демонстраційний, який є в репозиторії
 *  та містить приклад заповнення конфігурації.
 *  - у файл .gitignore додаємо запис emailconfig.json
 *  - створюємо у проєкті файл emailconfig.json
 *  - створюємо у проєкті файл emailconfig.sample.json
 *  = бажано перевірити - зробити коміт і переконатись, що лише один файл
 *     є у репозиторії - emailconfig.sample.json
 */
