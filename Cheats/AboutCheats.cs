using limm.Pages;

namespace limm.Cheats;

public class AboutCheats : IPageRegistrar
{
    public void Register()
    {
        ModMenu.RegisterPage(new DynamicPage(new PageDefinition
        {
            Id = "about",
            Title = "About",
            Order = 99,
            Categories =
            {
                new CategoryDefinition
                {
                    Name = "About",
                    Cheats =
                    {
                        (ctx, col) =>
                        {
                            ctx.AddParagraph("Len's Island Mod Menu template. Add new pages by creating an IPageRegistrar in the Cheats folder and calling ModMenu.RegisterPage(new DynamicPage(...)).");
                        }
                    }
                }
            }
        }));
    }
}
