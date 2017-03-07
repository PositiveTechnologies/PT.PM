package com.amdocs.uss.api.inac.service;

import com.amdocs.css.core.exceptions.AppException;
import com.amdocs.css.vip.fttb.data.AbstractConnectedFttbOfferEntityDO;
import com.amdocs.css.vip.fttb.data.AbstractFttbOfferEntityWithParamsDO;
import com.amdocs.css.vip.fttb.data.AnnualContractDiscountDO;
import com.amdocs.css.vip.fttb.data.FttbAnnualContractDO;
import com.amdocs.css.vip.fttb.data.FttbAnnualContractExtensionDO;
import com.amdocs.css.vip.fttb.data.IptvBaseTariffDO;
import com.amdocs.css.vip.fttb.data.IptvPacketDO;
import com.amdocs.css.vip.inac.data.InacServiceDO;
import com.amdocs.uss.api.inac.service.params.AnnualContractParamsApiDO;
import com.amdocs.uss.api.inac.service.params.BundleParamsApiDO;
import com.amdocs.uss.api.inac.service.params.DrWebParamsApiDO;
import com.amdocs.uss.api.inac.service.params.EsetParamsApiDO;
import com.amdocs.uss.api.inac.service.params.InacServiceAdditionalParamsApiDO;
import com.amdocs.uss.api.inac.service.params.IptvConsoleRentParamsApiDO;
import com.amdocs.uss.api.inac.service.params.IptvMultiRoomParamsApiDO;
import com.amdocs.uss.api.inac.service.params.IptvTariffBaseParamsApiDO;
import com.amdocs.uss.api.inac.service.params.KasperParamsApiDO;
import com.amdocs.uss.api.inac.service.params.NetPhoneParamsApiDO;
import com.amdocs.uss.api.inac.service.params.OttParamsApiDO;
import com.amdocs.uss.api.inac.service.params.StaticIpParamsApiDO;
import com.amdocs.uss.api.inac.service.params.TurboButtonBaseParamsApiDO;
import com.amdocs.uss.api.inac.service.params.VpdnParamsApiDO;
import com.amdocs.uss.api.inac.service.params.VsuParamsApiDO;
import com.amdocs.uss.api.inac.service.params.WifiRentParamsApiDO;
import com.amdocs.uss.api.inac.service.params.connected.FirewallParamsApiDO;
import com.fasterxml.jackson.annotation.JsonInclude;
import com.google.common.collect.Lists;
import com.wordnik.swagger.annotations.ApiModelProperty;
import org.apache.commons.collections.CollectionUtils;
import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import ru.atc.uss.exception.SystemServiceException;
import ru.atc.uss.fttb.service.AbstractFttbOfferEntityDO;
import ru.atc.uss.fttb.service.FttbOfferEntityType;

import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;

/**
 * @author Dmitriy Matveev <dmatveev@at-consulting.ru>
 *         Date: 9/25/14
 *         Time: 5:14 PM
 */
@XmlRootElement(name = "InacBaseServiceApiDO")
public class InacBaseServiceApiDO extends AbstractInacServiceApiDO {

    private static final long serialVersionUID = 2377481353937588197L;

    private static final Logger LOGGER = LoggerFactory.getLogger(InacBaseServiceApiDO.class);

    public static final Map<FttbOfferEntityType, Class<? extends InacServiceAdditionalParamsApiDO>> PARAMS_MAP = new HashMap<>();

    static {
        PARAMS_MAP.put(FttbOfferEntityType.DR_WEB, DrWebParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.STATIC_IP, StaticIpParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.ESET, EsetParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.KASPER, KasperParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.OTT, OttParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.VPDN, VpdnParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.ANNUAL_CONTRACT, AnnualContractParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.VSU_SERVICE, VsuParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.IPTV_TARIFF_ENTITY, IptvTariffBaseParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.NET_PHONE, NetPhoneParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.TURBO_BUTTON, TurboButtonBaseParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.IPTV_CONSOLE_RENT, IptvConsoleRentParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.FIREWALL, FirewallParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.IPTV_MULTIROOM, IptvMultiRoomParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.WIFI_RENT, WifiRentParamsApiDO.class);
        PARAMS_MAP.put(FttbOfferEntityType.BUNDLE, BundleParamsApiDO.class);
    }

    @SuppressWarnings("unchecked")
    public static InacServiceAdditionalParamsApiDO createServiceParams(FttbOfferEntityType type, InacServiceDO serviceDO,
                                                             AbstractFttbOfferEntityWithParamsDO baseService) {
        if (type == null) {
            return null;
        }
        Class<? extends InacServiceAdditionalParamsApiDO> paramsClass = PARAMS_MAP.get(type);
        if (paramsClass == null) {
            return null;
        }
        InacServiceAdditionalParamsApiDO result = null;
        try {
            result = paramsClass.newInstance();
            // В случае подключенных услуг возможно переопределение базовых параметров
            if (baseService != null) {
                result.initBaseParams(baseService);
            }
            if (serviceDO != null) {
                result.initConnectedParams(serviceDO);
            }
        } catch (Exception e) {
            LOGGER.error(e.getMessage(), e);
        }
        return result;
    }

    @SuppressWarnings("unchecked")
    public static <T> T createServiceParams(FttbOfferEntityType type, InacServiceDO serviceDO,
                                                                       AbstractFttbOfferEntityWithParamsDO baseService, Class<T> clazz) {
        return (T)createServiceParams(type, serviceDO, baseService);
    }

    public static InacServiceAdditionalParamsApiDO createServiceParams(FttbOfferEntityType type,
                                                                       AbstractFttbOfferEntityWithParamsDO baseService) {
        return createServiceParams(type, null, baseService);
    }

    @ApiModelProperty(value = "ID сервиса")
    @XmlElement
    @JsonInclude(JsonInclude.Include.ALWAYS) protected String serviceId;

    @ApiModelProperty(value = "Скидка в единицах валюты абонента. Параметр актуален только для услуг, находящихся внутри бандла." +
            " Валюта передается в методе /inac/account/contractInfo, параметр currency")
    @XmlElement
    @JsonInclude(JsonInclude.Include.NON_NULL) protected Double bundleDiscount;

    //TODO убрать в подкласс, когда сваггер сможет отображать структуру дочерних классов
    @ApiModelProperty(value = "Список тематических (премиальных) IPTV-пакетов (только для сервиса IPTV_TARIFF_ENTITY)")
    @XmlElement
    @JsonInclude(JsonInclude.Include.NON_NULL) protected List<InacIptvPacketApiDO> packetTypes;

    //TODO убрать в подкласс, когда сваггер сможет отображать структуру дочерних классов
    @ApiModelProperty(value = "Список скидок (только для сервиса ANNUAL_CONTRACT)")
    @XmlElement
    @JsonInclude(JsonInclude.Include.NON_NULL) protected List<InacAnnualContractDiscountApiDO> discounts;

    //дублируем в потомках параметры, т.к. описание в сваггере должно отличаться
    @ApiModelProperty(value = "Дополнительные параметры сервиса. В зависимости от типа сервиса," +
            " набор параметров следующий: <br><br>" +
            "<table>" +
            "<caption>Сервис 'Тариф ШПД' (Тип сервиса - 'VPDN')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Категория сервиса</td>" +
            "<td>serviceType</td>" +
            "<td>String</td>" +
            "<td>Справочник INAC</td>" +
            "<td>L - лимитный,<br/>" +
            "S - с понижением скорости,<br/>" +
            "U - безлимитный,<br/>" +
            "N - ночной с повышением скорости,<br/>" +
            "MULTI-SERVICE,<br/>" +
            "SHAPED-MULTI-SERVICE<br/>" +
            "</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Тип скидки</td>" +
            "<td>discountType</td>" +
            "<td>int</td>" +
            "<td>Справочник INAC</td>" +
            "<td>0 (без скидки), 1 (скидочный), 2 (оффсетный)</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Архивность</td>" +
            "<td>archive</td>" +
            "<td>boolean</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Объем входящего трафика, включенного в пакет, Мбайт</td>" +
            "<td>trafficVolumeIn</td>" +
            "<td>Long</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Цена в валюте абонента за входящий трафик за МБайт сверх пакета</td>" +
            "<td>vPriceOut</td>" +
            "<td>Double</td>" +
            "<td>Справочник INAC</td>" +
            "<td>Цена в рублях</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Скорость download (к абоненту) по умолчанию, Кбит/с</td>" +
            "<td>vSpeedIn</td>" +
            "<td>Long</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Скорость upload (от абонента) по умолчанию, Кбит/с</td>" +
            "<td>vSpeedOut</td>" +
            "<td>Long</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Скорость download (к абоненту) после порога/в ночное время, Кбит/с</td>" +
            "<td>vSpeedIn1</td>" +
            "<td>Long</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Скорость upload (от абонента) после порога/в ночное время, Кбит/с</td>" +
            "<td>vSpeedOut1</td>" +
            "<td>Long</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Срок действия в днях (признак оффсетного сервиса)</td>" +
            "<td>changeTime</td>" +
            "<td>Integer</td>" +
            "<td>Справочник INAC</td>" +
            "<td>офсет</td>" +
            "</tr>" +
            "<tr>" +
            "<td>На какой сервис (VPDN) будет осуществляться переход</td>" +
            "<td>changeTo1</td>" +
            "<td>String</td>" +
            "<td>Справочник INAC</td>" +
            "<td>ID тарифа</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Тип скидки</td>" +
            "<td>fcDiscountType</td>" +
            "<td>Integer</td>" +
            "<td>Справочник INAC</td>" +
            "<td>0 (кол-во рублей), 1 (кол-во %)</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Номинал скидки (в рублях/%)</td>" +
            "<td>fcDiscount</td>" +
            "<td>Double</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Период действия скидки, месяцев(биллинг-циклов)</td>" +
            "<td>fcDiscountBc</td>" +
            "<td>Integer</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Объем исходящего трафика, включенного в пакет, Мбайт</td>" +
            "<td>trafficVolumeOut</td>" +
            "<td>Long</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Тип соединения</td>" +
            "<td>connectType</td>" +
            "<td>String</td>" +
            "<td>Справочник INAC</td>" +
            "<td>PPTP, L2TP</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Признак тарифного плана для моб. абонентов</td>" +
            "<td>mobileSubscriber</td>" +
            "<td>boolean</td>" +
            "<td>Справочник INAC</td>" +
            "<td>0 - нет, 1 - да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Доступность подключения сервиса 'Годовой контракт'</td>" +
            "<td>annualContractAllowed</td>" +
            "<td>boolean</td>" +
            "<td>Справочник INAC</td>" +
            "<td>1 — доступны все ГК города и на данный момент доступные для подключения, 0 — никакие ГК не доступны." +
            "</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Доступность подключения сервиса 'Выбор скорости' (true - доступно, false - недоступно)</td>" +
            "<td>vsuAllowed</td>" +
            "<td>boolean</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Доступность связанного предложения ТВ</td>" +
            "<td>iptvRelatedOfferEnabled</td>" +
            "<td>boolean</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Доступность связанного предложения ДЦТ</td>" +
            "<td>netPhoneRelatedOfferEnabled</td>" +
            "<td>boolean</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Доступность связанного предложения OTT</td>" +
            "<td>ottRelatedOfferEnabled</td>" +
            "<td>boolean</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "<td>Нет</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Признак тарифа услуги поддержки линии связи.</td>" +
            "<td>lineHolder</td>" +
            "<td>boolean</td>" +
            "<td></td>" +
            "<td></td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Турбокнопка' (Тип сервиса - 'TURBO_BUTTON')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Признак отображения для условно-безлимитных сервисов VPDN</td>" +
            "<td>showIfShaping</td>" +
            "<td>boolean</td>" +
            "<td>Справочник INAC</td>" +
            "<td>не будет заполняться</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Признак PromoButton</td>" +
            "<td>promoButton</td>" +
            "<td>boolean</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Тип турбокнопки</td>" +
            "<td>turboButton</td>" +
            "<td>Integer</td>" +
            "<td>Справочник INAC</td>" +
            "<td>не будет заполняться</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Значение, определяющее изменение скорости</td>" +
            "<td>turboSpeedUp</td>" +
            "<td>Integer</td>" +
            "<td>Справочник INAC</td>" +
            "<td>новое значение скорости</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Увеличение порогового размера трафика, Мбайт</td>" +
            "<td>turboTraffic</td>" +
            "<td>Long</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Продолжительность действия услуги, кол-во часов</td>" +
            "<td>turboDuration</td>" +
            "<td>Double </td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Ограничение на ЧП-период  активации в ЛК</td>" +
            "<td>activationDuringRushHourAllowed</td>" +
            "<td>boolean</td>" +
            "<td>Справочник INAC</td>" +
            "<td>не будет заполняться</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Дата окончания действия (yyyy-MM-ddTHH:mm:ss.SSSZ). Параметр актуален только для подключенного сервиса</td>" +
            "<td>endTime</td>" +
            "<td>STRING</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Выбор скорости' (Тип сервиса - 'VSU_SERVICE')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Прибавка к скорости, Мбит/с</td>" +
            "<td>vpdnSpeedUp</td>" +
            "<td>Long</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Цена подключения сервиса (в валюте абонента)</td>" +
            "<td>vsuConnectFee</td>" +
            "<td>Double</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Цена перехода на более дорогой сервис (в валюте абонента)</td>" +
            "<td>vsuMoveUpFee</td>" +
            "<td>Double</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Цена перехода на более дешевый сервис (в валюте абонента)</td>" +
            "<td>vsuMoveDownFee</td>" +
            "<td>Double</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Максимальная скорость в городе, Мбит/с</td>" +
            "<td>vsuMaxSpeed</td>" +
            "<td>Long</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Дата активации (подключения) сервиса (yyyy-MM-ddTHH:mm:ss.SSSZ)</td>" +
            "<td>activationDate</td>" +
            "<td>Date</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Статический IP-адрес' (Тип сервиса - 'STATIC_IP')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>IP-адрес</td>" +
            "<td>ipAddress</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис аренды Wifi-роутера (Тип сервиса - 'WIFI_RENT')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Серийный номер роутера</td>" +
            "<td>routerSerial</td>" +
            "<td>String</td>" +
            "<td>Online из INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Модель роутера</td>" +
            "<td>routerModel</td>" +
            "<td>String</td>" +
            "<td>Online из INAC</td>" +
            "<td></td>" +
            "<td>да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Тип роутера. Характеристика сервиса аренды роутера, в зависимости от которой отбираются доступные для подключения " +
            "роутеры. Возможные значения: 0,1,2</td>" +
            "<td>routerType</td>" +
            "<td>Integer</td>" +
            "<td>Online из INAC</td>" +
            "<td></td>" +
            "<td>да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>В аренде/куплен</td>" +
            "<td>rent</td>" +
            "<td>boolean</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Признак типа собственности роутера</td>" +
            "<td>ownershipType</td>" +
            "<td>String</td>" +
            "<td>Справочник INAC</td>" +
            "<td>B - куплен, R - в аренде</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Mac-адрес роутера</td>" +
            "<td>mac</td>" +
            "<td>String</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "<td>да</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Мобильное телевидение' (Тип сервиса - 'OTT')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Разрешить использовать в связке</td>" +
            "<td>allowRelatedOfferByFttb</td>" +
            "<td>boolean</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Годовой контракт' (Тип сервиса - 'ANNUAL_CONTRACT')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Минимальная стоимость (в валюте абонента) сервиса Интернет, при которой доступен годовой контракт</td>" +
            "<td>acServParam</td>" +
            "<td>Double</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Срок контракта, кол-во месяцев (биллинг циклов)</td>" +
            "<td>acScaleSize</td>" +
            "<td>Integer</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Текущий месяц годового контракта. Параметр актуален только для подключенного сервиса</td>" +
            "<td>currentMonth</td>" +
            "<td>Integer</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Текущая цена сервиса VPDN. Параметр актуален только для подключенного сервиса</td>" +
            "<td>vpdnServicePrice</td>" +
            "<td>Double</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>" +
            "Возможная сумма штрафа, которая спишется со счета в случае прерывания ГК, " +
            "до активного цикла ГК соответствует сумме накопленной скидки, по истечении последнего месяца обнуляется. " +
            "Параметр актуален только для подключенного сервиса" +
            "</td>" +
            "<td>penalty</td>" +
            "<td>Double</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Текущий размер скидки в %. Параметр актуален только для подключенного сервиса</td>" +
            "<td>discount</td>" +
            "<td>Double</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Накопленная на текущий момент сумма скидки. Параметр актуален только для подключенного сервиса</td>" +
            "<td>currentSumDiscount</td>" +
            "<td>Double</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Домашний цифровой телефон (ДЦТ)' (Тип сервиса - 'NET_PHONE')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Разрешить использовать в связке</td>" +
            "<td>allowRelatedOfferByFttb</td>" +
            "<td>boolean</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Номер телефона</td>" +
            "<td>phone</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Никнейм</td>" +
            "<td>nickName</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Услуга Мультирум' (Тип сервиса - 'IPTV_MULTIROOM')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Идентификатор приставки</td>" +
            "<td>guid</td>" +
            "<td>String</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "<td>да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Mac-адрес приставки</td>" +
            "<td>mac</td>" +
            "<td>String</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "<td>да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Тип приставки</td>" +
            "<td>guidType</td>" +
            "<td>String</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "<td>да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Серийный номер приставки</td>" +
            "<td>serial</td>" +
            "<td>String</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "<td>да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Признак наличия жесткого диска у приставки, true- с жестким диском, false - без жесткого диска</td>" +
            "<td>hddExists</td>" +
            "<td>boolean</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Антивирус Касперского' (Тип сервиса - 'KASPER')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Кол-во лицензий</td>" +
            "<td>licenseCount</td>" +
            "<td>Integer</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Код продукта</td>" +
            "<td>productCode</td>" +
            "<td>String</td>" +
            "<td>Справочник INAC</td>" +
            "<td>Неинтерпретируемая строка – Код Продукта. Справочник кода продукта отсутствует.</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Серийный номер</td>" +
            "<td>serial</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Антивирус Dr. Web' (Тип сервиса - 'DR_WEB')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Кол-во лицензий</td>" +
            "<td>licenseCount</td>" +
            "<td>Integer</td>" +
            "<td>Справочник INAC</td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Код продукта</td>" +
            "<td>productCode</td>" +
            "<td>String</td>" +
            "<td>Справочник INAC</td>" +
            "<td>Неинтерпретируемая строка – Код Продукта. Справочник кода продукта отсутствует.</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Ссылка для скачивания (только для подключенного сервиса)</td>" +
            "<td>url</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Антивирус Eset' (Тип сервиса - 'ESET')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Код продукта</td>" +
            "<td>productCode</td>" +
            "<td>Integer</td>" +
            "<td>Справочник INAC</td>" +
            "<td>Неинтерпретируемая строка – Код Продукта. Справочник кода продукта отсутствует.</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Логин</td>" +
            "<td>esetUsername</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Пароль</td>" +
            "<td>esetPassword</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Кол-во лицензий</td>" +
            "<td>licenseCount</td>" +
            "<td>Integer</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td></td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Firewall' (Тип сервиса - 'FIREWALL')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Уровень файрвола</td>" +
            "<td>level</td>" +
            "<td>int</td>" +
            "<td>Справочник INAC</td>" +
            "<td>" +
            "0 - DISABLED (отключен), " +
            "1 - AVERAGE (средний), " +
            "2 - MAXIMUM (максимальный), " +
            "10- AVERAGE_AND_BLOCK_SMTP (средний + блокировка SMTP)" +
            "</td>" +
            "<td>да</td>" +
            "</tbody>" +
            "</table>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Аренда ТВ-приставки' (Тип сервиса - 'IPTV_CONSOLE_RENT')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Идентификатор приставки</td>" +
            "<td>guid</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Mac-адрес приставки</td>" +
            "<td>mac</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Тип приставки</td>" +
            "<td>guidType</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Серийный номер приставки</td>" +
            "<td>serial</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Флаг возможности остановки аренды при входе в блокировку</td>" +
            "<td>nonStop</td>" +
            "<td>boolean</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Признак наличия жесткого диска у приставки, true- с жестким диском, false - без жесткого диска</td>" +
            "<td>hddExists</td>" +
            "<td>boolean</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "</table>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Дополнительные параметры ТВ-тарифа' (Тип сервиса - 'IPTV_TARIFF_ENTITY')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Эффективная цена в единицах валюты абонента. Валюта передается в методе /inac/account/contractInfo, " +
            "параметр currency, .руб</td>" +
            "<td>effectivePrice</td>" +
            "<td>Double</td>" +
            "<td></td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Признак тарифного плана для моб. абонентов</td>" +
            "<td>mobileSubscriber</td>" +
            "<td>boolean</td>" +
            "<td></td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Разрешить использовать в связке</td>" +
            "<td>allowRelatedOfferByFttb</td>" +
            "<td>boolean</td>" +
            "<td></td>" +
            "<td></td>" +
            "</tr>" +
            "<tr>" +
            "<td>Идентификатор приставки</td>" +
            "<td>guid</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Mac-адрес приставки</td>" +
            "<td>mac</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Тип приставки</td>" +
            "<td>guidType</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Серийный номер приставки</td>" +
            "<td>serial</td>" +
            "<td>String</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "<tr>" +
            "<td>Признак наличия жесткого диска у приставки, true- с жестким диском, false - без жесткого диска</td>" +
            "<td>hddExists</td>" +
            "<td>boolean</td>" +
            "<td></td>" +
            "<td></td>" +
            "<td>Да</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Платный возврат IPTV' (Тип сервиса - 'IPTV_PAID_COURIER_CALL')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<tr>" +
            "<td>Стоимость сервиса</td>" +
            "<td>price</td>" +
            "<td>Double</td>" +
            "<td></td>" +
            "<td></td>" +
            "</tr>" +
            "</table>" +
            "<br><table>" +
            "<caption>Сервис 'Дополнительные параметры бандла' (Тип сервиса - 'BUNDLE')</caption>" +
            "<thead>" +
            "<tr>" +
            "<th>Описание параметра</th>" +
            "<th>Название параметра</th>" +
            "<th>Тип значения параметра</th>" +
            "<th>Источник данных</th>" +
            "<th>Комментарий</th>" +
            "<th>Параметр актуален только для подключенных услуг</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            "<tr>" +
            "<td>Минимальная стоимость бандла в формате double. Цена в единицах валюты абонента. " +
            "Валюта передается в методе /inac/account/contractInfo, параметр currency</td>" +
            "<td>minPriceBundle</td>" +
            "<td>Double</td>" +
            "<td></td>" +
            "<td></td>" +
            "</tr>" +
            "</tbody>" +
            "</table>", required = true
    )
    @XmlElement
    @JsonInclude(JsonInclude.Include.ALWAYS) protected InacServiceAdditionalParamsApiDO additionalParams;

    public InacBaseServiceApiDO() {

    }

    public InacBaseServiceApiDO(AbstractFttbOfferEntityDO inacService) {
        this.technicalName = inacService.getName();
        this.price = inacService.getPrice();
        if (inacService.getOfferId() != null) {
            this.serviceId = StringUtils.lowerCase(inacService.getOfferId().toString());
        }
    }

    public InacBaseServiceApiDO(InacBaseServiceApiDO baseServiceApiDO) {
        super(baseServiceApiDO);
        this.serviceId = baseServiceApiDO.serviceId;
        this.bundleDiscount = baseServiceApiDO.bundleDiscount;
        //предполагается, что копирования не требуется, т.к. входной объект после вызова данного конструктора не будет использоваться
        this.packetTypes = baseServiceApiDO.packetTypes;
        this.discounts = baseServiceApiDO.discounts;
        this.additionalParams = baseServiceApiDO.additionalParams;
    }

    //TODO подумать, как объединить с другим initBaseAnnualContractInfo
    protected void initBaseAnnualContractInfo(FttbAnnualContractDO contractDO) {
        discounts = Lists.newArrayList();
        if (contractDO == null || CollectionUtils.isEmpty(contractDO.getDiscounts())) {
            return;
        }
        for (AnnualContractDiscountDO discountDO : contractDO.getDiscounts()) {
            discounts.add(new InacAnnualContractDiscountApiDO(discountDO));
        }
        AnnualContractDiscountDO maxAnnualContractDiscountDO = new AnnualContractDiscountDO();
        maxAnnualContractDiscountDO.setPrice(contractDO.getPriceForMaxDiscountPercent());
        maxAnnualContractDiscountDO.setDiscountPercent(contractDO.getMaxDiscountPercent());
        discounts.add(new InacAnnualContractDiscountApiDO(maxAnnualContractDiscountDO));
    }

    protected void initBaseAnnualContractInfo(FttbAnnualContractExtensionDO extensionDO) {
        discounts = Lists.newArrayList();
        if (extensionDO == null || CollectionUtils.isEmpty(extensionDO.getDiscounts())) {
            return;
        }
        for (AnnualContractDiscountDO discountDO : extensionDO.getDiscounts()) {
            discounts.add(new InacAnnualContractDiscountApiDO(discountDO));
        }
        AnnualContractDiscountDO maxAnnualContractDiscountDO = new AnnualContractDiscountDO();
        maxAnnualContractDiscountDO.setPrice(extensionDO.getPriceForMaxDiscountPercent());
        maxAnnualContractDiscountDO.setDiscountPercent(extensionDO.getMaxDiscountPercent());
        discounts.add(new InacAnnualContractDiscountApiDO(maxAnnualContractDiscountDO));
    }

    public static InacIptvPacketApiDO buildIptvPacketsApiStructure(IptvPacketDO packet, Set<Long> connectedIds)
            throws AppException, SystemServiceException {
        if (packet == null) {
            return null;
        }
        InacIptvPacketApiDO packetApiDO = new InacIptvPacketApiDO(packet, connectedIds);
        if (CollectionUtils.isEmpty(packet.getChildren())) {
            return packetApiDO;
        }
        for (IptvPacketDO child : packet.getChildren()) {
            packetApiDO.addChildPacket(buildIptvPacketsApiStructure(child, connectedIds));
        }
        return packetApiDO;
    }

    protected void initBaseIptvPacketInfo(IptvBaseTariffDO packet, Set<Long> connectedIds) throws AppException, SystemServiceException {
        setPacketTypes(null);
        if (packet == null) {
            return;
        }
        List<InacIptvPacketApiDO> packetTypesResult = Lists.newArrayList();
        for (IptvPacketDO packetType: packet.getPacketTypes()) {
            packetTypesResult.add(buildIptvPacketsApiStructure(packetType, connectedIds));
        }
        setPacketTypes(packetTypesResult);
    }

    protected void initBaseServiceParams(AbstractFttbOfferEntityWithParamsDO inacService, AbstractConnectedFttbOfferEntityDO connected) {
        initServiceDescriptionParams(inacService);
        InacServiceAdditionalParamsApiDO result = null;
        if (inacService == null) {
            result = createServiceParams(connected.getOfferEntityType(), connected.getConnectionInfo(), null);
        } else if (connected == null) {
            result = createServiceParams(inacService.resolveType(), inacService);
        }  else {
            result = createServiceParams(connected.getOfferEntityType(), connected.getConnectionInfo(), inacService);
        }
        setAdditionalParams(result);
    }

    protected void initBaseServiceParams(AbstractFttbOfferEntityWithParamsDO inacService) {
        initBaseServiceParams(inacService, null);
    }

    public String getServiceId() {
        return serviceId;
    }

    public void setServiceId(String serviceId) {
        this.serviceId = StringUtils.lowerCase(serviceId);
    }

    public List<InacIptvPacketApiDO> getPacketTypes() {
        return packetTypes;
    }

    public void setPacketTypes(List<InacIptvPacketApiDO> packetTypes) {
        this.packetTypes = packetTypes;
    }

    public List<InacAnnualContractDiscountApiDO> getDiscounts() {
        return discounts;
    }

    public void setDiscounts(List<InacAnnualContractDiscountApiDO> discounts) {
        this.discounts = discounts;
    }

    public Double getBundleDiscount() {
        return bundleDiscount;
    }

    public void setBundleDiscount(Double bundleDiscount) {
        this.bundleDiscount = bundleDiscount;
    }

    public InacServiceAdditionalParamsApiDO getAdditionalParams() {
        return additionalParams;
    }

    public void setAdditionalParams(InacServiceAdditionalParamsApiDO additionalParams) {
        this.additionalParams = additionalParams;
    }
}
