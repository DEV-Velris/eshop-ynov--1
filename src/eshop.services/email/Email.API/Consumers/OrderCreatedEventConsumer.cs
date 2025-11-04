using BuildingBlocks.Messaging.Events;
using Email.API.Services;
using MassTransit;
using System.Globalization;

namespace Email.API.Consumers;

/// <summary>
/// Consumer qui écoute l'événement OrderCreatedEvent et envoie un email de confirmation
/// </summary>
public class OrderCreatedEventConsumer(
    IEmailService emailService,
    ILogger<OrderCreatedEventConsumer> logger) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var orderEvent = context.Message;
        
        logger.LogInformation("📧 OrderCreatedEvent received for order {OrderId}", orderEvent.OrderId);
        
        try
        {
            var subject = $"Confirmation de commande #{orderEvent.OrderName}";
            var body = BuildOrderConfirmationEmail(orderEvent);
            
            await emailService.SendEmailAsync(
                to: orderEvent.CustomerEmail,
                subject: subject,
                body: body
            );
            
            logger.LogInformation("✅ Order confirmation email sent to {Email}", orderEvent.CustomerEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Failed to send order confirmation email for order {OrderId}", orderEvent.OrderId);
            throw;
        }
    }
    
    private static string BuildOrderConfirmationEmail(OrderCreatedEvent orderEvent)
    {
        var frenchCulture = new CultureInfo("fr-FR");
        
        var itemsHtml = string.Join("", orderEvent.OrderItems.Select(item => 
            $@"<tr>
                <td>Produit #{item.ProductId}</td>
                <td style='text-align: center;'>{item.Quantity}</td>
                <td>{item.Price.ToString("C", frenchCulture)}</td>
                <td style='font-weight: 600;'>{(item.Price * item.Quantity).ToString("C", frenchCulture)}</td>
            </tr>"));

        return $@"
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            margin: 0; 
            padding: 16px; 
            background-color: #f8f9fa; 
        }}
        .header {{ 
            background: linear-gradient(135deg, #2563eb, #1d4ed8); 
            color: white; 
            padding: 30px 20px; 
            text-align: center; 
            box-shadow: 0 2px 10px rgba(37, 99, 235, 0.2); 
        }}
        .header h1 {{ 
            margin: 0; 
            font-size: 28px; 
            font-weight: 600; 
        }}
        .container {{ 
            padding: 16px;
            max-width: 600px; 
            margin: 0 auto; 
            background-color: white; 
            border-radius: 12px; 
            overflow: hidden; 
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); 
        }}
        .content {{ 
            padding: 30px; 
        }}
        .greeting {{ 
            font-size: 20px; 
            color: #1f2937; 
            margin-bottom: 20px; 
            font-weight: 500; 
        }}
        .order-info {{ 
            background-color: #f1f5f9; 
            border-radius: 8px; 
            padding: 20px; 
            margin: 20px 0; 
            border-left: 4px solid #2563eb; 
        }}
        .summary-table {{ 
            width: 100%; 
            border-collapse: collapse; 
            margin: 20px 0; 
            background-color: white; 
            border-radius: 8px; 
            overflow: hidden; 
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1); 
        }}
        .summary-table th {{ 
            background-color: #2563eb; 
            color: white; 
            padding: 15px; 
            text-align: left; 
            font-weight: 600; 
        }}
        .summary-table td {{ 
            padding: 12px 15px; 
            border-bottom: 1px solid #e5e7eb; 
        }}
        .items-table {{ 
            width: 100%; 
            border-collapse: collapse; 
            margin: 20px 0; 
            background-color: white; 
            border-radius: 8px; 
            overflow: hidden; 
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1); 
        }}
        .items-table th {{ 
            background-color: #475569; 
            color: white; 
            padding: 12px; 
            text-align: left; 
            font-weight: 600; 
        }}
        .items-table td {{ 
            padding: 12px; 
            border-bottom: 1px solid #e5e7eb; 
        }}
        .items-table tr:nth-child(even) {{ 
            background-color: #f8fafc; 
        }}
        .thank-you {{ 
            color: #059669; 
            font-weight: 600; 
            margin: 25px 0; 
            text-align: center; 
            font-size: 18px; 
        }}
        .footer {{ 
            text-align: center; 
            color: #6b7280; 
            margin-top: 30px; 
            padding-top: 20px; 
            border-top: 1px solid #e5e7eb; 
            font-size: 14px; 
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Confirmation de Commande</h1>
        </div>
        
        <div class='content'>
            <div class='greeting'>
                Bonjour <strong>{orderEvent.CustomerName}</strong> ! 👋
            </div>
            
            <p>Votre commande <strong>#{orderEvent.OrderId}</strong> a été créée avec succès !</p>
            
            <div class='order-info'>
                <h3 style='margin-top: 0; color: #2563eb;'>📋 Récapitulatif de votre commande</h3>
                <table class='summary-table'>
                    <tr>
                        <th>Information</th>
                        <th>Détails</th>
                    </tr>
                    <tr>
                        <td><strong>ID Commande</strong></td>
                        <td>{orderEvent.OrderId}</td>
                    </tr>
                    <tr>
                        <td><strong>Date de commande</strong></td>
                        <td>{orderEvent.OrderDate:dd/MM/yyyy à HH:mm}</td>
                    </tr>
                    <tr>
                        <td><strong>Montant total</strong></td>
                        <td style='font-weight: 600; color: #059669; font-size: 16px;'>{orderEvent.TotalPrice.ToString("C", frenchCulture)}</td>
                    </tr>
                </table>
            </div>
            
            <h3 style='color: #2563eb; margin-top: 30px;'>Articles commandés</h3>
            <table class='items-table'>
                <thead>
                    <tr>
                        <th>Produit</th>
                        <th>Quantité</th>
                        <th>Prix unitaire</th>
                        <th>Total</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsHtml}
                </tbody>
            </table>
            
            <div class='thank-you'>
                Merci pour votre confiance ! 🛒
            </div>
            
            <div class='footer'>
                <p><em>Ceci est un email automatique généré par notre système e-commerce.</em></p>
                <p>Si vous avez des questions, n'hésitez pas à nous contacter.</p>
            </div>
        </div>
    </div>
</body>
</html>";
    }
}