using BuildingBlocks.Messaging.Events;
using Email.API.Services;
using MassTransit;
using System.Globalization;

namespace Email.API.Consumers;

/// <summary>
/// Consumer qui écoute l'événement OrderDeletedEvent et envoie un email de confirmation
/// </summary>
public class OrderDeletedEventConsumer(
    IEmailService emailService,
    ILogger<OrderDeletedEventConsumer> logger) : IConsumer<OrderDeletedEvent>
{
    public async Task Consume(ConsumeContext<OrderDeletedEvent> context)
    {
        var orderEvent = context.Message;

        logger.LogInformation("📧 OrderDeletedEvent received for order {OrderId}", orderEvent.OrderId);

        try
        {
            var subject = $"Notification de suppression de commande #{orderEvent.OrderName}";
            var body = BuildOrderDeletionEmail(orderEvent);

            await emailService.SendEmailAsync(
                to: orderEvent.CustomerEmail,
                subject: subject,
                body: body
            );

            logger.LogInformation("✅ Order deletion notification email sent to {Email}", orderEvent.CustomerEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Failed to send order deletion email for order {OrderId}", orderEvent.OrderId);
            throw;
        }
    }

    private static string BuildOrderDeletionEmail(OrderDeletedEvent orderEvent)
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
            padding: 10px; 
            background-color: #f8f9fa; 
        }}
        .header {{ 
            background: linear-gradient(135deg, #dc3545, #b02a37); 
            color: white; 
            padding: 30px 20px; 
            text-align: center; 
            box-shadow: 0 2px 10px rgba(220, 53, 69, 0.2); 
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
        .deletion-banner {{
            padding: 20px;
            border-radius: 12px;
            margin: 20px 0;
            text-align: center;
            font-size: 18px;
            font-weight: 600;
            color: white;
            background-color: #dc3545;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        }}
        .deletion-icon {{
            font-size: 24px;
            margin-bottom: 8px;
        }}
        .order-info {{ 
            background-color: #f1f5f9; 
            border-radius: 8px; 
            padding: 20px; 
            margin: 20px 0; 
            border-left: 4px solid #dc3545; 
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
            background-color: #dc3545; 
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
        .footer {{ 
            text-align: center; 
            color: #6b7280; 
            margin-top: 30px; 
            padding-top: 20px; 
            border-top: 1px solid #e5e7eb; 
            font-size: 14px; 
        }}
        .important-note {{
            background-color: #fef3cd;
            border: 1px solid #ffeaa7;
            border-radius: 8px;
            padding: 15px;
            margin: 20px 0;
            color: #856404;
        }}
        .important-note strong {{
            color: #dc3545;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🗑️ Suppression de commande</h1>
        </div>
        
        <div class='content'>
            <div class='greeting'>
                Bonjour <strong>{orderEvent.CustomerName}</strong> ! 👋
            </div>
            
            <p>Nous vous informons que votre commande <strong>#{orderEvent.OrderId}</strong> a été supprimée de notre système.</p>
            
            <div class='deletion-banner'>
                <div class='deletion-icon'>🗑️</div>
                <div>Commande supprimée</div>
            </div>
            
            <div class='important-note'>
                <strong>Important :</strong> Cette action est définitive. Si vous pensez qu'il s'agit d'une erreur ou si vous souhaitez passer une nouvelle commande, n'hésitez pas à nous contacter.
            </div>
            
            <div class='order-info'>
                <h3 style='margin-top: 0; color: #dc3545;'>📋 Détails de la commande supprimée</h3>
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
                        <td><strong>Date de suppression</strong></td>
                        <td>{orderEvent.UpdatedDate:dd/MM/yyyy à HH:mm}</td>
                    </tr>
                    <tr>
                        <td><strong>Montant total</strong></td>
                        <td style='font-weight: 600; color: #dc3545; font-size: 16px;'>{orderEvent.TotalPrice.ToString("C", frenchCulture)}</td>
                    </tr>
                </table>
            </div>
            
            <h3 style='color: #dc3545; margin-top: 30px;'>🛍️ Articles de la commande supprimée</h3>
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
            
            <div class='footer'>
                <p><em>Ceci est un email automatique généré par notre système e-commerce.</em></p>
                <p>Si vous avez des questions ou des préoccupations, n'hésitez pas à nous contacter.</p>
                <p><strong>Cordialement,<br/>L'équipe E-Shop</strong></p>
            </div>
        </div>
    </div>
</body>
</html>";
    }
}