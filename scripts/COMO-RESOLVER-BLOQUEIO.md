# Como resolver o bloqueio "Controle de Aplicativo"

O Windows está bloqueando o `ExpenseWeb.exe` por política de segurança. Siga **uma** das opções abaixo:

---

## Opção 1: Usar o perfil "executable" (contorno automático)

Foi adicionado um perfil que usa `dotnet.exe` (assinado) em vez do `ExpenseWeb.exe`:

1. No Visual Studio, no menu **Iniciar** (ou barra de ferramentas)
2. Abra o dropdown dos perfis de inicialização
3. Selecione **executable** em vez de "https" ou "http"
4. Pressione F5

Isso evita o bloqueio sem mexer nas configurações do Windows.

---

## Opção 2: Desativar Smart App Control (recomendado para desenvolvimento)

1. Abra **Configurações** (Win + I)
2. **Privacidade e segurança** → **Segurança do Windows** → **Proteção contra vírus e ameaças**
3. Role até **Configurações de proteção contra vírus e ameaças**
4. Clique em **Proteção controlada por aplicativos** (Smart App Control)
5. Desative ou altere para "Avaliar"

---

## Opção 3: Adicionar exclusão no Windows Defender

Execute o arquivo `ExecutarComoAdmin.bat` nesta pasta (clique duplo e aceite a permissão de administrador).

*Nota: Essa exclusão pode não afetar o Controle de Aplicativo/Smart App Control.*
